"""Publish a complete APK plus reusable content chunks; no third-party dependencies."""
from pathlib import Path
import argparse,hashlib,json,shutil
MIN,MAX=32768,262144
GEAR=[(((i+1)*2654435761)^((i+17)*2246822519))&0xffffffff for i in range(256)]
def chunks(path):
    data=Path(path).read_bytes();start=0;rolling=0
    for index,value in enumerate(data):
        rolling=((rolling<<1)+GEAR[value])&0xffffffff
        length=index-start+1
        if length>=MIN and (rolling&65535==0 or length==MAX):
            block=data[start:index+1];yield start,block;start=index+1;rolling=0
    if start<len(data):yield start,data[start:]
def publish(apk,folder,version,code):
    folder=Path(folder);store=folder/'chunks';store.mkdir(parents=True,exist_ok=True)
    entries=[]
    for offset,block in chunks(apk):
        digest=hashlib.sha256(block).hexdigest();target=store/(digest+'.bin')
        if not target.exists():target.write_bytes(block)
        elif hashlib.sha256(target.read_bytes()).hexdigest()!=digest:raise ValueError('Corrupt existing chunk')
        entries.append({'sha256':digest,'size':len(block)})
    content=Path(apk).read_bytes()
    manifest={'version':version,'versionCode':code,'size':len(content),'sha256':hashlib.sha256(content).hexdigest(),'chunking':'gear32-v1','chunks':entries}
    rebuilt=b''.join((store/(entry['sha256']+'.bin')).read_bytes() for entry in entries)
    if rebuilt!=content:raise ValueError('Reconstruction failed')
    shutil.copy2(apk,folder/'ArtificialNegligence-latest.apk')
    (folder/'latest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    print(f'Published {version}: {len(content):,} bytes, {len(entries)} verified chunks')
    return manifest
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('apk',type=Path);parser.add_argument('--version',required=True);parser.add_argument('--code',type=int,required=True);parser.add_argument('--folder',type=Path,default=Path(__file__).resolve().parents[1]/'apk')
    args=parser.parse_args()
    if args.code<1:parser.error('Version code must be positive')
    previous=args.folder/'latest.json'
    if previous.exists():
        last=json.loads(previous.read_text())
        if args.code<last['versionCode']:parser.error('Cannot publish an older version code')
        if args.code==last['versionCode'] and hashlib.sha256(args.apk.read_bytes()).hexdigest()!=last['sha256']:parser.error('Changed APK needs a higher version code')
    publish(args.apk,args.folder,args.version,args.code)
