"""Publish small balance updates without an APK, music or art download."""
from pathlib import Path
import argparse,json,math
FIELDS=('enemyHealth','bossHealth','enemySpeed','enemyDamage','weaponDamage','xp')
parser=argparse.ArgumentParser()
parser.add_argument('--version',required=True,type=int)
parser.add_argument('--notes',default='Gameplay balance update.')
parser.add_argument('--folder',type=Path,default=Path(__file__).resolve().parents[1]/'apk')
for name in FIELDS:parser.add_argument('--'+name,type=float)
args=parser.parse_args();path=args.folder/'gameplay.json'
current=json.loads(path.read_text()) if path.exists() else {'version':0,**{key:1 for key in FIELDS}}
if args.version<=current['version']:parser.error('Increase gameplay version for each published balance update')
for key in FIELDS:
    value=getattr(args,key)
    if value is not None:
        if not math.isfinite(value) or not .25<=value<=4:parser.error(key+' must be 0.25 to 4')
        current[key]=value
current.update(version=args.version,notes=args.notes[:280])
args.folder.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps(current,indent=2)+'\n',encoding='utf-8')
print('Published gameplay balance version',args.version,'-',path.stat().st_size,'bytes. Commit and push gameplay.json.')
