"""Publish range-update metadata and an optional single legacy bridge; no dependencies."""
from pathlib import Path
import argparse,hashlib,json
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
def publish_ranges(apk,folder,version,code,url,bridge=False):
    from urllib.parse import urlparse
    parsed=urlparse(url)
    if parsed.scheme!='https' or parsed.netloc!='github.com' or not parsed.path.startswith('/dustindwayne/artificial-negligence/releases/download/'):
        raise ValueError('Use the immutable GitHub Release APK URL')
    folder=Path(folder);folder.mkdir(parents=True,exist_ok=True)
    previous=folder/'latest-v2.json'
    content=Path(apk).read_bytes();digest=hashlib.sha256(content).hexdigest()
    if previous.exists():
        last=json.loads(previous.read_text())
        if code<last['versionCode'] or code==last['versionCode'] and digest!=last['sha256']:raise ValueError('Changed APK requires a higher version code')
    entries=[]
    for offset,block in chunks(apk):entries.append({'sha256':hashlib.sha256(block).hexdigest(),'size':len(block)})
    manifest={'protocol':2,'version':version,'versionCode':code,'size':len(content),'sha256':digest,'downloadUrl':url,'chunking':'gear32-v1','chunks':entries}
    pending=folder/'latest-v2.json.tmp';pending.write_text(json.dumps(manifest,indent=2)+'\n');pending.replace(previous)
    if bridge:
        store=folder/'chunks';store.mkdir(exist_ok=True);keep=set()
        for offset,block in chunks(apk):
            name=hashlib.sha256(block).hexdigest()+'.bin';keep.add(name);target=store/name
            if not target.exists():target.write_bytes(block)
            elif hashlib.sha256(target.read_bytes()).hexdigest()!=name[:-4]:raise ValueError('Corrupt bridge chunk')
        legacy={k:v for k,v in manifest.items() if k not in ('protocol','downloadUrl')}
        pending=folder/'latest.json.tmp';pending.write_text(json.dumps(legacy,indent=2)+'\n');pending.replace(folder/'latest.json')
        removed=0;removed_bytes=0
        for file in store.glob('*.bin'):
            if file.name not in keep:removed+=1;removed_bytes+=file.stat().st_size;file.unlink()
        print(f'Bridge cleanup: removed {removed} outdated chunks ({removed_bytes:,} bytes)')
    obsolete=folder/'ArtificialNegligence-latest.apk'
    if obsolete.exists():obsolete.unlink()
    (folder/'latest.url').write_text(url+'\n')
    print(f'Range manifest {version}: one Release APK, {len(entries)} reusable regions, {len(content):,} bytes')
    return manifest

if __name__=='__main__':
    parser=argparse.ArgumentParser(description='Publish update metadata for one immutable GitHub Release APK.')
    parser.add_argument('apk',type=Path);parser.add_argument('--version',required=True);parser.add_argument('--code',type=int,required=True)
    parser.add_argument('--folder',type=Path,default=Path(__file__).resolve().parents[1]/'apk');parser.add_argument('--download-url',required=True)
    parser.add_argument('--bridge',action='store_true',help='Create/replace the single legacy upgrade bridge and prune unreferenced chunks.')
    args=parser.parse_args()
    if args.code<1:parser.error('Version code must be positive')
    publish_ranges(args.apk,args.folder,args.version,args.code,args.download_url,args.bridge)
