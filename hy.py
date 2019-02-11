#!/usr/bin/python3
import urllib.request
import argparse
import json

parser = argparse.ArgumentParser(description='Submit hash or url to hybrid analysis')
parser.add_argument("-H","--Hash", help='hash for submission', type=str)
parser.add_argument("-U","--url", help='url for submission', type=str)
parser.add_argument("-f","--File", help='hashes for submission', type=str)
args = parser.parse_args()
Hash = str(args.Hash)
Url = str(args.url)
infile = str(args.File)

h = {"accept":"application/json","Content-Type":"application/x-www-form-urlencoded","user-agent":"Falcon Sandbox","api-key":"<ENTER HERE>"}

def hsh():
        req = urllib.request.Request('https://www.hybrid-analysis.com/api/v2/search/hash', headers = h)
        d = {"hash":Hash}
        data = urllib.parse.urlencode(d)
        data = data.encode('ascii')
        req.data = data
        req.method = 'POST'
        resp = urllib.request.urlopen(req)
        html = resp.read()
        html = html.decode("utf-8")
        return html.replace(',','\n')

def readfile():
        f = open(infile,"r")
        data = f.read()
        split = data.split()
        return split

def bulkhash(hashes):
        req = urllib.request.Request('https://www.hybrid-analysis.com/api/v2/search/hashes', headers = h)
        d = {"hashes":hashes}
        data = urllib.parse.urlencode(d)
        data = data.encode('ascii')
        req.data = data
        req.method = 'POST'
        resp = urllib.request.urlopen(req)
        html = resp.read()
        html = html.decode("utf-8")
        return html.replace(',','\n')

def urlsubmit():
        d = {"url":Url,"environment_id":"120","no_share_third_party":"true","allow_community_access":"false"}
        req = urllib.request.Request('https://www.hybrid-analysis.com/api/v2/submit/url-for-analysis', headers = h)
        data = urllib.parse.urlencode(d)
        data = data.encode('ascii')
        req.data = data
        req.method = 'POST'
        resp = urllib.request.urlopen(req)
        html = resp.read()
        html = html.decode("utf-8")
        html = json.loads(html)
        jobid = html['job_id']
        return jobid
        #return html.replace(',','\n')

def getreport(jobid):
        req = urllib.request.Request('https://www.hybrid-analysis.com/api/v2/report/' + jobid + '/summary', headers = h)
        req.method = 'GET'
        resp = urllib.request.urlopen(req)
        html = resp.read()
        html = html.decode("utf-8")
        return html.replace(',','\n')

def main():
        if args.Hash:
              if hsh() == '[]':
                   print("No Hybrid Analysis Summary Available.")
              else:
                   print(hsh())
        elif args.File:
              hashes = readfile()
              print(bulkhash(hashes))
        elif args.url:
              jobid = urlsubmit()
              print(getreport(jobid))

main()
