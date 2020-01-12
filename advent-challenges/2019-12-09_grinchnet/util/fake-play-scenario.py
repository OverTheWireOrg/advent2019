#!/usr/bin/env python

from pwn import *
import sys

#context.log_level = 'DEBUG'
password = "GRINCHRULES!bBad"


scenariofile = sys.argv[1]
persona = sys.argv[2]
port = sys.argv[3]

def readline(p, name):
    rout = ""
    logr = log.progress("[%s] Reading" % name)
    while True:
        c = p.read(1)
        if c == "\n":
            break
        rout += c
        logr.status(rout)
    logr.success(rout)

def typeline(p, name, line, delay=0):
    rout = ""
    tout = ""
    logt = log.progress("[%s] Typing" % name)
    #logr = log.progress("[%s] Reading" % name)
    for i in line:
        p.send(i)
        tout += i
        rout += p.read(1)
        logt.status(tout)
        #logr.status(rout)
    p.send('\n')
    p.read(1)
    logt.success(tout)
    time.sleep(delay)
    #logr.success()

if __name__ == "__main__":
    fullscenario = [x.strip() for x in open(scenariofile).readlines()]

    if persona == "start":
        initCall = True
    else:
        initCall = False

    conn = serialtube(port=port, baudrate=9600, convert_newlines=False)

    conn.recvuntil("Are you [E]xpecting or [I]nitiating a call?\r\n")
    if initCall:
        conn.send("i")
    else:
        conn.send("e")

    conn.recvuntil("Enter key for encryption: ")
    typeline(conn, "me", password)

    if initCall:
        raw_input("Ready to start. Press enter to continue")
        conn.recvuntil(">>>")

        for line in fullscenario:
            typeline(conn, "me", line, delay=3)
    else:
        while True:
            readline(conn, "other")


    print "Done."

    conn.interactive()


