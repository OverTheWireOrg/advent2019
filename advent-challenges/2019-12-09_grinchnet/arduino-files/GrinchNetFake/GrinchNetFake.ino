/*

first line: init communication as either master or slave, plus a passphrase

encode data as RC4
send data as pulses

*/

#define DATAPIN 12
#define SIGNALPIN 13
#define BITDELAY 100

bool initialized = false;
bool expectingData = false;
char encKey[32];

unsigned char S[256];
unsigned int rc4_i, rc4_j;

void initConnection() {
  char cmd;
  Serial.println("Are you [E]xpecting or [I]nitiating a call?");
  while(1 != Serial.readBytes(&cmd, 1));
  
  if(cmd == 'I' || cmd == 'i') {
      Serial.println("OK, you will initiate a call.");
      initialized = true;
      expectingData = false;
      return;
  }
  if(cmd == 'E' || cmd == 'e') {
      Serial.println("OK, you will expect a call.");
      initialized = true;
      expectingData = true;
      return;
  }
  Serial.println("I don't understand.");
  initialized = false;
}

/* read a key into the given buffer
 *  strips newlines
 *  null-terminates buffer
 *  returns length of key
 */
size_t getKey(char *buf, size_t buflen) {
  size_t i;
  Serial.print("Enter key for encryption: ");

  buf[buflen - 1] = '\x00';
  for(i = 0; i < buflen-1; i++) {
      while(1 != Serial.readBytes(&buf[i], 1));

      //Serial.println(buf[i], DEC);
      if(buf[i] == '\n') {
        buf[i] = '\x00';
        goto done;
      }
      Serial.print("*");
  }

done:
  Serial.println();
  return i;
}

bool isValidKey(char *key) {
  unsigned char *x = (unsigned char *)key;
  String kstr = String(key);
  if(kstr.length() != 16) return false;

  if(x[0] != x[5] - 1) return false;
  if((x[2]^0x42) != 11) return false;
  if((x[3]^x[5]) != 6) return false;
  if((x[4] - 1) != (x[11] * 2)) return false;
  if(x[5] != (x[2] - 1)) return false;
  if(x[6] ^ x[1]) return false;
  if(((x[7] >> 4) * (x[7] & 0xf)) != 25) return false;
  if(x[8] != (x[2] + 3)) return false;
  if(x[9] != (x[4] + 2)) return false;
  if(x[10] != (x[7] - 2)) return false;
  if(((x[11] >> 4) | ((x[11] << 4) & 0xff)) != 0x12) return false;
  //if(!kstr.startsWith("GRINCHRULES!")) return false;
  
  for(size_t i = 12; i < 16; i++) if(!isHexadecimalDigit(key[i])) return false;

  return true;
}

#define SWAP(a,b) {unsigned char tmp=a; a=b; b=tmp;}
void rc4_init(char *key) {
  for(unsigned int i = 0; i < 256; i++) {
    S[i] = i;
  }
  unsigned int j = 0;
  for(unsigned int i = 0; i < 256; i++) {
    j = (j + S[i] + (unsigned char)key[i % 16]) % 256;
    SWAP(S[i], S[j]);
  }
  rc4_i = 0;
  rc4_j = 0;
}

unsigned char rc4_crypt(unsigned char x) {
  rc4_i = (rc4_i + 1) % 256;
  rc4_j = (rc4_j + S[rc4_i]) % 256;
  SWAP(S[rc4_i], S[rc4_j]);
  unsigned char k = S[(S[rc4_i] + S[rc4_j]) % 256];
  return k ^ x;
}

void gpio_write(unsigned char x) {
  for(int i = 7; i >= 0; i--) {
    unsigned int b = (1 << i) & x;
    digitalWrite(DATAPIN, b == 0 ? LOW : HIGH);
    digitalWrite(SIGNALPIN, HIGH);
    delay(BITDELAY);
    digitalWrite(SIGNALPIN, LOW);
    delay(BITDELAY);
  }
}

bool gpio_read_bit() {
  while(digitalRead(SIGNALPIN) == LOW);
  auto val = digitalRead(DATAPIN);
  while(digitalRead(SIGNALPIN) == HIGH);
  return val == HIGH;
}

unsigned char gpio_read() {
  unsigned char out = '\x00';
  for(int i = 7; i >= 0; i--) {
    unsigned int b = (1 << i);
    if(gpio_read_bit()) out |= b;
  }
  return out;
}

void config_current_mode(bool m) {
  expectingData = m;
  if(expectingData) {
    pinMode(DATAPIN, INPUT);
    pinMode(SIGNALPIN, INPUT);
    Serial.println("==[ Current mode: expecting data.");
    Serial.print("<<< ");
  } else {
    pinMode(DATAPIN, OUTPUT);
    pinMode(SIGNALPIN, OUTPUT);  
    Serial.println("==[ Current mode: sending data.");
    Serial.print(">>> ");
  }
}

void setup() {
  bool keyok = false;
  // initialize both serial ports:
  Serial.begin(9600);
  Serial.println();
  Serial.println("~={ Welcome to GrinchNet }=~");
  Serial.println();

  while(!initialized) initConnection();

  while(!keyok) {
    getKey(encKey, sizeof(encKey));
    
    Serial.print("Your key: [");
    Serial.print(encKey);
    Serial.println("]");

    keyok = isValidKey(encKey);
    if(keyok) {
      Serial.println("OK, that looks like a good key.");
    } else {
      Serial.println("That is *NOT* an approved key! Try again.");
    }
  }
  Serial.println();

  rc4_init(encKey);
  config_current_mode(expectingData);
}



void loop() {
  /*
   * wait for a byte either from serial, or from the GPIO pin
   * then encode or decode it
   * and output to the serial or GPIO
   * if it's a newline, then switch mode
   */
   unsigned char inb, outb;
   
   if(expectingData) {
    inb = gpio_read();
    outb = rc4_crypt(inb);
    Serial.print((char)outb);
    //Serial.println(inb, BIN);
    /*
    if(outb == '\n') {
      Serial.println();
      config_current_mode(false);
    }
    */
   } else {
    if (Serial.available()) {
      inb = (unsigned char)Serial.read();
      outb = rc4_crypt(inb);
      gpio_write(outb);
      Serial.print((char)inb);
      //Serial.println(outb, BIN);
      /*
      if(inb == '\n') {
        Serial.println();
        config_current_mode(true);
      }
      */
    }    
   }
}
