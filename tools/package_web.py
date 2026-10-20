"""Restore the themed page after BuildWeb.Build and package for FTP hosting."""
from pathlib import Path
from zipfile import ZipFile,ZIP_DEFLATED
import argparse,json,shutil

def package(build,source,out):
 build=Path(build);source=Path(source);out=Path(out)
 def pick(suffix):
  files=list((build/'Build').glob('*'+suffix))
  if len(files)!=1:raise ValueError('Expected exactly one current file for '+suffix)
  return 'Build/'+files[0].name
 config={'dataUrl':pick('.data.unityweb'),'frameworkUrl':pick('.framework.js.unityweb'),'codeUrl':pick('.wasm.unityweb'),'streamingAssetsUrl':'StreamingAssets','companyName':'Artificial Negligence','productName':'Artificial Negligence','productVersion':'2.4.9','autoSyncPersistentDataPath':True}
 html=(source/'web/index.template.html').read_text(encoding='utf-8').replace('__UNITY_CONFIG__',json.dumps(config)).replace('__LOADER_URL__',json.dumps(pick('.loader.js')))
 css=(source/'web/style.css').read_text(encoding='utf-8')
 html=html.replace('<link rel="stylesheet" href="style.css">','<style>'+css+'</style>')
 (build/'index.html').write_text(html,encoding='utf-8');(build/'style.css').write_text(css,encoding='utf-8')
 shutil.copy2(source/'docs/looper-vs-wrath-banner.png',build/'hero.png')
 (build/'.htaccess').write_text('<IfModule mod_mime.c>\n AddType application/octet-stream .unityweb\n</IfModule>\n<IfModule mod_headers.c>\n <FilesMatch "^index\\.html$">\n  Header set Cache-Control "no-cache"\n </FilesMatch>\n</IfModule>\n',encoding='utf-8')
 (build/'Build/.htaccess').write_text('<IfModule mod_mime.c>\n AddType application/octet-stream .unityweb\n</IfModule>\n<IfModule mod_headers.c>\n Header set Cache-Control "public, max-age=31536000, immutable"\n</IfModule>\n',encoding='utf-8')
 assert '__UNITY_CONFIG__' not in html and '__LOADER_URL__' not in html
 assert 'https://ko-fi.com/stonerautist' in html and 'autoSyncPersistentDataPath' in html
 out.parent.mkdir(parents=True,exist_ok=True)
 with ZipFile(out,'w',ZIP_DEFLATED,compresslevel=5) as z:
  for f in build.rglob('*'):
   if f.is_file():z.write(f,Path('play')/f.relative_to(build))
  guide=(source/'docs/WEB-HOSTING.md').read_text(encoding='utf-8').replace('(CONTROLS.md)','(https://github.com/dustindwayne/artificial-negligence/blob/main/docs/CONTROLS.md)')
  z.writestr('UPLOAD-INSTRUCTIONS.md',guide)
 with ZipFile(out) as z:assert z.testzip() is None
 print('Web FTP package verified:',out.stat().st_size,'bytes')
 return html,css
if __name__=='__main__':
 parser=argparse.ArgumentParser();parser.add_argument('--build',default='ArtificialNegligence-Web');parser.add_argument('--source',default='.');parser.add_argument('--out',default='ArtificialNegligence-Web-FTP.zip');args=parser.parse_args();package(args.build,args.source,args.out)
