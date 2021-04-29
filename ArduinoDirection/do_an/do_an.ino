int distance1;
int distance2;
 int distance3;//biến lƣu khoảng cách
const int dirR=4;
byte AnaR =5;
const int dirL=2;
byte AnaL=3;
const int encoder0PinAL=21;
const int encoder0PinBL=24;
const int encoder0PinAR=20;
const int encoder0PinBR=22;
int encoder0PosL=0;
int encoder0PosR=0;
float PulseLeft = 0;
float PulseRight=0;
int k=0;//bien di toi
int countR=0;
int countL=0;
int m=0;
int giatri=0;
int khoangcach;
const int trig1 = 36;//chân trig của HC-SR04
const int echo1 = 38;//chân echo của HC-SR04
const int trig2 = 40;//chân trig của HC-SR04
const int echo2 = 42;//chân echo của HC-SR04
const int trig3 = 32;//chân trig của HC-SR04
const int echo3 = 34;//chân echo của HC-SR04
unsigned long time;
int xungkc;
String dulieu;
void doeL()//do encoder trai
{
    if(digitalRead(encoder0PinBL) == LOW)
      countL++;
    else
      countL--;
}
void doeR()//do encoder phai
{
    if(digitalRead(encoder0PinBR) == LOW)
      countR++;
    else
      countR--;
}

void setup() {
  // put your setup code here, to run once:
pinMode(dirL, OUTPUT);        // Set chan xuat pwm dieu khien dong co ben trai
pinMode(AnaL, OUTPUT);
pinMode(dirR, OUTPUT);        // Set chan xuat pwm dieu khien dong co ben trai
pinMode(AnaR, OUTPUT);
pinMode(encoder0PinAL, INPUT_PULLUP);
       // turn on pullup resistor
pinMode(encoder0PinBL, INPUT_PULLUP);
      // turn on pullup resistor
pinMode(encoder0PinAR, INPUT_PULLUP);
       // turn on pullup resistor
pinMode(encoder0PinBR, INPUT_PULLUP);
pinMode(trig1,OUTPUT);//chân trig sẽ phát tín hiệu
 pinMode(echo1,INPUT);//chân echo sẽ nhận tín hiệu
 pinMode(trig2,OUTPUT);//chân trig sẽ phát tín hiệu
 pinMode(echo2,INPUT);//chân echo sẽ nhận tín hiệu
 pinMode(trig3,OUTPUT);//chân trig sẽ phát tín hiệu
 pinMode(echo3,INPUT);//chân echo sẽ nhận tín hiệu
Serial.begin(9600);//giao tiếp Serial với baudrate 9600
 attachInterrupt(3, doeR, FALLING); 
 attachInterrupt(2, doeL, FALLING); 
 dulieu="";
}
void loop(){
 /* while (Serial.available())
  { 
    String data=Serial.read();//du lieu cong serial
     char mangdata[data.length()+1]=data;
  }
if(k==0){
  dilui();
  delay(9000);
  
  quaytrai90();
  quaytrai90();
  dilui();
  delay(9000);
  quaytrai90();
  quaytrai90();
  digitalWrite(dirR, 1);analogWrite(AnaR,0);
        digitalWrite(dirL, 0);analogWrite(AnaL,0);
  k=1;
}*/
/*if(k==0){
xungkc=1667;
autogo();
analogWrite(AnaR,0);
analogWrite(AnaL,0);
Serial.println(countL);
Serial.println(giatri);
k=1;
}*/
/*if(k==0){
quaytrai90();k=1;}
else{
digitalWrite(dirR, 1);analogWrite(AnaR,0);
        digitalWrite(dirL, 1);analogWrite(AnaL,0);
}*/
char data = Serial.read();
if (data == 'q'){ sstop();//q
} // d?ng
else if (data == 'w'){ ditoi();//w
} //ti?n
else if (data == 's'){ dilui();//s
} //lùi
else if (data == 'd'){ quaytrai90();//d
}//trái
else if (data == 'a'){ quayphai90(); 
}
else{
  if(data=='r') k=1;
  else{
    if(data>47&&data<58){
       dulieu = dulieu+data ; 
      }
   
    }
 
}
if(k==1){
khoangcach=dulieu.toInt();
xungkc=khoangcach*555.5;
autogo();
}
}
void quayphai90()
{
  digitalWrite(dirR, 1);analogWrite(AnaR,70);
        digitalWrite(dirL, 1);analogWrite(AnaL,70);
        delay(1750);
analogWrite(AnaR,0);analogWrite(AnaL,0);
}
void quaytrai90()
{
  digitalWrite(dirR, 0);analogWrite(AnaR,70);
        digitalWrite(dirL, 0);analogWrite(AnaL,70);
delay(1750);
analogWrite(AnaR,0);analogWrite(AnaL,0);
}
void ditoi()
{
digitalWrite(dirR, 1);analogWrite(AnaR,70);
        digitalWrite(dirL, 0);analogWrite(AnaL,70);
        
}
void dilui()
{
digitalWrite(dirR, 0);
        digitalWrite(dirL, 1);analogWrite(AnaR,70);analogWrite(AnaL,70);
        
}
void CBtruoc()
{
 unsigned long duration1=0;//biến đo thời gian
 //biến lƣu khoảng cách
 /* phát xung từ chân trig */
 digitalWrite(trig1,0);//tắt chân trig
 delayMicroseconds(2);
 digitalWrite(trig1,1);// phát xung từ chân trig
 delayMicroseconds(5);// xung có độ dài 5 microSeconds
 digitalWrite(trig1,0);//tắt chân trig
 /*tính toán thời gian*/
 duration1 = pulseIn(echo1,HIGH);//đo độ rộng xung HIGH ở chân echo.
 distance1 = int(duration1/2/29.412);//tính khoảng cách đến vật.
 if(distance1==0||distance1>300) distance1=300;
}
void CBtrai()
{
 unsigned long duration2=0;//biến đo thời gian
 //biến lƣu khoảng cách
 /* phát xung từ chân trig */
 digitalWrite(trig2,0);//tắt chân trig
 delayMicroseconds(2);
 digitalWrite(trig2,1);// phát xung từ chân trig
 delayMicroseconds(5);// xung có độ dài 5 microSeconds
 digitalWrite(trig2,0);//tắt chân trig
 /*tính toán thời gian*/
 duration2 = pulseIn(echo2,HIGH);//đo độ rộng xung HIGH ở chân echo.
 distance2 = int(duration2/2/29.412);//tính khoảng cách đến vật.
 if(distance2==0||distance2>300) distance2=300;
}
void CBphai()
{
 unsigned long duration3=0;//biến đo thời gian
 /* phát xung từ chân trig */
 digitalWrite(trig3,0);//tắt chân trig
 delayMicroseconds(2);
 digitalWrite(trig3,1);// phát xung từ chân trig
 delayMicroseconds(5);// xung có độ dài 5 microSeconds
 digitalWrite(trig3,0);//tắt chân trig
 /*tính toán thời gian*/
 duration3 = pulseIn(echo3,HIGH);//đo độ rộng xung HIGH ở chân echo.
 distance3 = int(duration3/2/29.412);//tính khoảng cách đến vật.
 if(distance3==0||distance3>300) distance3=300;
}
void sstop(){
analogWrite(AnaL,0);analogWrite(AnaR,0); 
}
void autogo() {
 while(xungkc>(abs(countL)+abs(countR))/2){
  CBtruoc();CBtrai();CBphai();
  if(distance1>30) {
    ditoi();
  }
  else{
    if((distance2<30)&&(distance3<30)){
      while((distance2<30)&&(distance3<30)){
       dilui(); 
      CBtrai();CBphai();
      }
      delay(3000);
      analogWrite(AnaL,0);analogWrite(AnaR,0);
      
      if((distance2>30)) {
      quaytrai90();
      ditoi();  
      delay(4000);
      xungkc=xungkc+320;
      quayphai90();
      ditoi();
      delay(6500);
      quayphai90();
      ditoi();
      delay(4000);
      xungkc=xungkc+320;
      quaytrai90();
    }
      else{
        delay(20);
        CBtruoc();
        if(distance2>30){
      quayphai90();
      ditoi();  
      delay(4000);
      xungkc=xungkc+320;
      quaytrai90();
      ditoi();
      delay(6500);
      quaytrai90();
      ditoi();
      delay(4000);
      xungkc=xungkc+320;
      quayphai90(); 
        }
    }
    } 
    else{
     if((distance2>30)) {
      quaytrai90();
      ditoi();  
      delay(4000);
      xungkc=xungkc+320;
      quayphai90();
      ditoi();
      delay(6500);
      quayphai90();
      ditoi();
      delay(4000);
      xungkc=xungkc+320;
      quaytrai90();
    }
      else{
      quayphai90();
      ditoi();  
      delay(4000);
      xungkc=xungkc+320;
      quaytrai90();
      ditoi();
      delay(6500);
      quaytrai90();
      ditoi();
      delay(4000);
      xungkc=xungkc+320;
      quayphai90();
    
    } 
    }
 }
 }
 analogWrite(AnaL,0);analogWrite(AnaR,0);
}

