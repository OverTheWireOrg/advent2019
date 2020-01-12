openssl rsautl -encrypt -inkey public.pem -pubin -in reward.txt | base64 > reward.b64
