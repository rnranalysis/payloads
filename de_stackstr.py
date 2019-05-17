import re
import argparse
 
parser = argparse.ArgumentParser(description='')
parser.add_argument("-f", "--file", type=str, required=True)
args = parser.parse_args()

f = open(args.file,"rb")
data = f.read()
f.close()

pattern = "\xC6\x85.\xFC\xFF\xFF."
pattern2 = "\xC6\x85.\xF2\xFF\xFF."

matches = re.findall(pattern, data)
matches2 = re.findall(pattern2, data)
formatted = ''
for match in matches:
	formatted += re.sub("\xC6\x85.\xFC\xFF\xFF","", match)
for match in matches2:
	formatted += re.sub("\xC6\x85.\xF2\xFF\xFF","",match)
print(re.sub("\x00","\n",formatted))
