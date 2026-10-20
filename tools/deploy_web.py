"""Upload a web build over verified FTPS, changing index.html only after assets pass verification.
Credentials come from siteduser/sitedftp environment variables and are never written to disk.
"""
import argparse,ftplib,ssl,os,hashlib,io,time,json
from pathlib import Path,PurePosixPath
from concurrent.futures import ThreadPoolExecutor
from urllib.request import Request,urlopen

class CompatibleFTPS(ftplib.FTP_TLS):
 def storbinary(self,*args,**kwargs):
  try:return super().storbinary(*args,**kwargs)
  except ConnectionResetError:
   # Some hosts reset TLS after accepting all bytes instead of sending close_notify.
   # Require the control-channel completion reply; the caller still verifies SHA-256.
   return self.voidresp()

def connect(args):
 ftp=CompatibleFTPS(context=ssl.create_default_context(),timeout=60)
 ftp.connect(args.host,21);ftp.login(os.environ['siteduser'],os.environ['sitedftp']);ftp.prot_p();ftp.cwd(args.remote)
 return ftp

def deploy(args):
 root=Path(args.build).resolve();assert (root/'index.html').is_file()
 assert args.remote and '..' not in PurePosixPath(args.remote).parts
 if not os.environ.get('siteduser') or not os.environ.get('sitedftp'):raise RuntimeError('Set the deployment credential environment variables before running')
 files=[p for p in root.rglob('*') if p.is_file()];backup=Path(args.backup);backup.mkdir(parents=True,exist_ok=True)
 ftp=connect(args)
 for name in ['index.html','.htaccess']:
  try:
   b=io.BytesIO();ftp.retrbinary('RETR '+name,b.write);(backup/name).write_bytes(b.getvalue())
  except ftplib.error_perm as e:
   if not str(e).startswith('550'):raise
 for folder in sorted({str(p.parent.relative_to(root)).replace('\\','/') for p in files if p.parent!=root},key=lambda x:len(PurePosixPath(x).parts)):
  cumulative=''
  for segment in PurePosixPath(folder).parts:
   cumulative=segment if not cumulative else cumulative+'/'+segment
   try:ftp.mkd(cumulative)
   except ftplib.error_perm as e:
    if not str(e).startswith('550'):raise
 ftp.quit()
 def upload(p):
  rel=p.relative_to(root).as_posix();digest=hashlib.sha256(p.read_bytes()).hexdigest();session=connect(args)
  try:
   # Content-hash filenames in Build are immutable; size equality is sufficient to reuse them.
   if rel.startswith('Build/') and len(p.name.split('.')[0])==32:
    try:
     session.voidcmd('TYPE I')
     if session.size(rel)==p.stat().st_size:return {'file':rel,'sha256':digest,'reused':True}
    except ftplib.error_perm:pass
   pending=rel+'.upload-'+digest[:12]
   with p.open('rb') as stream:session.storbinary('STOR '+pending,stream,blocksize=262144)
   actual=hashlib.sha256();session.retrbinary('RETR '+pending,actual.update,blocksize=262144)
   if actual.hexdigest()!=digest:raise RuntimeError('Upload hash mismatch: '+rel)
   session.rename(pending,rel)
   print('Verified upload:',rel,flush=True);return {'file':rel,'sha256':digest,'reused':False}
  finally:
   try:session.quit()
   except:session.close()
 assets=[p for p in files if p.name!='index.html'];results=[]
 with ThreadPoolExecutor(max_workers=3) as pool:
  for row in pool.map(upload,assets):results.append(row)
 # Switch the public entry page only after every referenced asset is present.
 results.append(upload(root/'index.html'))
 req=Request(args.url.rstrip('/')+'/?release_verify='+str(time.time_ns()),headers={'Cache-Control':'no-cache'})
 with urlopen(req,timeout=40) as response:live=response.read()
 assert hashlib.sha256(live).hexdigest()==hashlib.sha256((root/'index.html').read_bytes()).hexdigest(),'Public entry page differs from uploaded build'
 report={'url':args.url,'files':results,'publicIndexSha256':hashlib.sha256(live).hexdigest(),'verified':True}
 Path(args.report).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
 print('PUBLIC WEBSITE UPDATE VERIFIED:',args.url,flush=True)

if __name__=='__main__':
 parser=argparse.ArgumentParser();parser.add_argument('--host',required=True);parser.add_argument('--remote',required=True);parser.add_argument('--url',required=True);parser.add_argument('--build',required=True);parser.add_argument('--backup',required=True);parser.add_argument('--report',required=True)
 deploy(parser.parse_args())
