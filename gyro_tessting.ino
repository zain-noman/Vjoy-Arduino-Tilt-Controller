#include<Wire.h>
float ThetaX=0,ThetaY=0,ThetaZ=0;
float deltaTime=0;
float Taw=10;

void setup() {
  Serial.begin(9600);
  Wire.begin();
  delay(20);
  
  Wire.beginTransmission(0x68);
  Wire.write(0x6B);
  Wire.write(0x00);   //power management register turn on code
  Wire.endTransmission(true);

  Wire.beginTransmission(0x68);
  Wire.write(0x1B);
  Wire.write(B00011000);    //gyro full scale range +-1000rad/s
  Wire.endTransmission(true);

  Wire.beginTransmission(0x68);
  Wire.write(0x1C);
  Wire.write(B00010000);    //accelerometer full scale range +-4g
  Wire.endTransmission(true);
}

void loop() 
{
  unsigned long m1 = millis();
  ReadGyroData();
  Serial.print(ThetaX);Serial.print("|");Serial.print(ThetaY);Serial.print("|");  Serial.println(ThetaZ);
  delay(100);
  
  deltaTime=float(millis()-m1)/1000;      //in seconds
}

void ReadGyroData()
{

  //setting up to read from Acceleometer
  Wire.beginTransmission(0x68);
  Wire.write(0x3B);
  Wire.endTransmission(false);

  Wire.requestFrom(0x68,14,true);
  int Ax = (Wire.read()<<8|Wire.read());
  int Ay = (Wire.read()<<8|Wire.read());
  int Az = (Wire.read()<<8|Wire.read());
  Wire.read();Wire.read();                //skipping temprature readings
  int Gx = (Wire.read()<<8|Wire.read());
  int Gy = (Wire.read()<<8|Wire.read());
  int Gz = (Wire.read()<<8|Wire.read());

  float ThetaGx = ThetaX+float(Gx)*(0.06103515624)*(deltaTime);
  float ThetaGy = ThetaY+float(Gy)*(0.06103515624)*(deltaTime);
  
  ThetaZ = (Taw/(Taw+deltaTime))*(ThetaZ+ (float(Gz)*(0.06103515624)*(deltaTime)));       //HighPass filter
    
  float ThetaAcX=atan(Ax/Az)*57.2957795131;
  float ThetaAcY=atan(Ay/Az)*57.2957795131;

  ThetaX=0.95*ThetaGx+0.05*ThetaAcX;
  ThetaY=0.95*ThetaGy+0.05*ThetaAcY;
}
