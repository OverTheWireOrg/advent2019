# The bugs

First, I will explain what the crypto scheme is and what the vulnerability is. Then I'll talk about how things are discovered.

The protocol works as follows: first the server sends a 16-byte key K. Then, both the client and the server will encrypt by xoring with K and a separate, fixed, stream S of bytes. In other words,

For each direction:
    ciphertext[i] = plaintext[i] xor K[i % 16] xor S[i]

S is unknown, but shared between the client and the server.

Once decrypted, the protocol works by sending _messages_. Each message is a 2 byte header encoding the message length in bytes, followed by a Google Protobuf encoding of the message. The protobuf schema is in server/Protos/game.proto, but the schema is not disclosed to the player.

There is a flag on the server and a flag on the client.

* The client flag is actually embedded in the client's binary, we just need to make it display an item that is normally not attainable. Items are numbered by ID. IDs 0 to 5 are weapons, 6 is HP potion, and 8 is the scroll of secrets, and 7, which is never sent by the server, is the client flag. Faking the server response by sending an inventory containing item 7 will display the flag on the client side.
* The server flag is in the "Scroll of Secrets" once the player is able to obtain it and use it. For this the player needs to have enough money. This money can be earned by either botting the game (killing on the order of a million monsters, which is probably doable, but actually fairly difficult to get right), or by exploiting an item duplication bug. The way items are transferred between backpack (0), stash (1), and shop (2) is by sending a transferring message from inventory ID to another inventory ID. If we transfer from the same inventory to the same inventory, the backend duplicates the item (this is a common bug I've seen on a real online game before, and we see this kind of bugs in real life like with TowelRoot). With this, one can pretty easily duplicate more and more high-end swords to eventually reach 10M coins (takes minimal effort).

# How to solve the crypto part

This part is really about setting up the right tooling to be able to efficiently explore and find patterns. If we play with the client UI, we see that we have a 16-byte username limit and 256-byte password limit. The password field gives us a giant user-controlled piece of data to play with.

First, there's the random key K. The server would send a random K every time, but if we fake the server and look at the client, the client will always send deterministic messages for a given K. So, for example, we can try sending the client all zeros for K, login with some inputs, then look at client message. Then, change one byte of K and look at the client message again. Instead of being totally different, the client message would only differ by a few bytes, exactly every 16 bytes. This suggests that K is simply being xor-ed onto the data rather than being fed through some cipher or being used as a key to anything.

Now, K is 16 bytes long, but if we login with a 256-byte password, we still don't see the client message having repeated 16-byte blocks, so there's some other encryption going on other than the xor-ing with K. No problem, we try a simple differential analysis again: once again fake the server and send K = zeros, but login twice with two passwords that differ by only 1 byte. We see that the client message differing by exactly one byte, suggesting that the client message is encrypted by xor-ing with a stream cipher (rather than, e.g. going through a block cipher like AES-CBC).

So now we have K and a stream cipher. There's one more thing to check - is the stream cipher applied independently per message or cumulatively? We can notice that after a failed login we can actually login again, so if we send in the same login messages in succession, during the same connection, we can see that the messages sent are completely different, suggesting that the stream cipher is cumulative for the connection.

So far we've only looked at the messages sent by the client, not by the server yet. But for the client, we have a good guess that the message is encrypted by xor-ing with a repeated key generated from K, and then xor-ing with a fixed deterministic stream cipher that is cumulative during the connection.

Since the stream cipher is fixed, it would be beneficial to figure out what its bytes are. Pretend to be the server again and, send two logins, one with password "A"\*256 and another with password "B"\*256. The client messages differ by exactly 256 bytes and from this we can mark which 256 bytes are different. If we then xor the password with the encrypted bytes, we get the stream cipher bytes at those locations (around bytes 20 - 276). That leaves us a gap at the beginning but we'll fill them in later.

Let's take these bytes of the stream cipher and xor with what the server sends (after also xor-ing with K of course). We can use a few incorrect login sessions, or just any server data (like the big blob that's sent on a successful login). We will see that we recover these 256 bytes properly - either plain text (in the case of login error messages), or non-random bytes. This suggests that the stream cipher is applied independently on each directions of the connection.

So now we can try to recover more of the keystream. This can be done in a couple of ways. For example, we can trigger multiple failed-login attempts (via the real client) and since we can recover the full failed-login server message by looking at the second or third such message, we can use it to recover the first 20-or-so bytes of the keystream (since the failed-login message is the same). We can repeat this to recover more of the keystream but this requires manual clicking on the client which is both slow and tedious. We can do a little better by faking the server and capturing client _login_ events which are longer (>256 bytes). These login events are also the same, so it allows us to keep xor-ing with the encrypted messages to recover more keystream bytes.

Still, this will only let us recover a few KB of the keystream, since it still requires manual clicking. To recover more keystream bytes we would need to login to the actual game. There are a few things one can do to get a longer response than request. The most efficient is probably to retrieve an item from the stash when the backpack is already full - the retrieval message is about 6 bytes long but the response message is O(~100) bytes (which are always the same bytes), so we only need to send a few thousand of these messages to be able to recover 1MB of keystream (which is the upper limit enforced by the server anyway).

At this point we're able to communicate freely with the server or the client without worrying about encryption.

# How to decode the protocol

This part either requires some manual differential analysis or knowing that popular protocol formats exist like Google Protobuf. There is a protobuf decoder here: https://protogen.marcgravell.com/decode. We don't have the schema but it's good enough. There are only 3 in-game messages the client sends: transfer item (this is the important one), battle, and use item. By trying different scenarios it shouldn't be too hard to understand the client-side messages even without knowing it's protocol buffer.

# TODO(hpmv): Fill in the rest.




