# Naughty List

## Description

A simple web application that encrypts all exposed GET parameters to
prevent tampering. The purpose of the application is a simple sign up
and a mechanism to transfer credits from one user to another.

Although the encryption looks like it might be vulnerable to the usual
suspects web hackers encounter, such as bit flipping values, block
forgery with `CBC` or a plain padding oracle, it is actually `GCM` and
so it has a tag that is more or less an HMAC for the stream cipher so
it becomes very difficult to attack with these methods as long as the
IV is different for each and every encryption (which it is). Timing
attacks are also limited in this specific scenario.

There are three bugs that can be used together in order to compromise:

### Read Oracle

On the sign up page, if there was a referer then this is encrypted and
put in the `redirect` `GET` parameter. The issue here is the cleartext
version of this `redirect` parameter is put in the form as a hidden
field. This means the user can arbitrarily decrypt the various encrypted
values seen around the website.


### Write Oracle

If you try alter the `page` parameter (something web hackers will do
instinctively, even when encrypted, since it looks like an LFI, perhaps
with a padding oracle or block forgery), they will get redirected to a
`404 Not Found` page with a `from_page` parameter in the URL. This is
the encrypted value of whatever they put in that `page` variable.

This means now the attacker can arbitrarily encrypt values without
having to know anything further about the encryption method used on the
website.

### Race Condition

Once they log in they will be presented with an account page that shows
they have `1` credit and they cannot buy more. They can however transfer
to another user. This should make them think of a race condition, the
only issue is they will need a `destination code` which is just an encrypted
string with the example being `sento:santa`. Of course the `santa` account
is taken by default, but they should be able to put the pieces together or
go look for the oracles in order to read this value and create their own.

Once they have gotten that far, they can transfer to another account they
own, with a very generous race condition that still feels real. Due to a
TOCTTOU they can transfer their credits multiple times with parallel requests.

Once they reach the goal amount the flag is dumped on all pages.

## Exploitation

In order to exploit, you need to sign up with two accounts, you will then
need to construct the `sendto:accountname` encrypted destination codes for
these accounts as well as get the `PHPSESSID`.

You can then use the simple helper exploit that uses the following args:
```
usage: ./ex.py <session> <amount> <destination code> [threads=10]
```

For example:
```bash
./ex.py 5f1db32eaa69a2390d316265a6025b85 1000 xZHrAF0FtxZNS0wQbGJjTk5vRy9IZm1jckE9PQkJoFLk7_UOHPgoKqQfTXs 50
```

It will then race and you can then send whatever credits are in that second
account back and forth between both accounts until you reach the goal.