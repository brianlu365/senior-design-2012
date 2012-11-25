#include <ZumoMotors.h>
// ============ OPERATIONAL CONSTANTS =================

// Create ZumoMotors objects
ZumoMotors motors;

// Create compass
//LSM303 compass;
//LSM303::vector running_min = {2047, 2047, 2047}, running_max = {-2048, -2048, -2048};

// Assign Wixel I/O to Arduino  pins
#define rf_pin  0           //  Arduino RX pin to read RF input from Wixel pin P1_6 (TX pin)


char go_direction = 'f' ;  // stop
char new_go_direction;
int lspeed = 0;
int rspeed = 0;

void setup() 
{   
  Serial.begin(57600);
  pinMode(rf_pin, INPUT);    // set Arduino RX pin for input from Wixel's TX pin
  motors.flipLeftMotor(true);

} 

void loop()
{ 
  // w = Forward  d = Right  a = Left  s = Back  f = Stop 
  //Serial.println("START loop");
//  while(Serial.available() <=0);
  if (Serial.available() > 0)
  {
    go_direction = Serial.read();
    move_to(go_direction);
  }
  else  
  {   
    move_to(new_go_direction);
  }
  
}

// ==================MOVEMENT FUCTION =====================================
// w = Forward  d = Right  a = Left  s = Back  f = Stop 

char move_to(char dir){
  switch (dir) { // scan ahead
  case 'w':  
    //Serial.println("move_to>> Turn  forward");
    go_forward();
    new_go_direction = 'w';
    break;
  case 'a': 
    //Serial.println("move_to>> Turn  Left");
    go_left();
    new_go_direction = 'a';
    break;
  case 'd': 
    //Serial.println("move_to>> Turn  Right");
    go_right();
    new_go_direction = 'd';
    break; 
  case 's': 
    //Serial.println("move_to>> Back up in straight line");
    go_reverse();
    new_go_direction = 's';
    break;
  case 'f': 
    //Serial.println("move_to>> Stop");
    go_still();
    new_go_direction = 'f';
    break;
  }
}

// ================== PHYSICAL MOVEMENT ========================

void go_forward()
{
//  while (lspeed < 200 || rspeed < 200)
//  {
//    if (lspeed < 200)
//      lspeed +=50;
//    else if (rspeed < 200)
//      rspeed +=50;
//    motors.setSpeeds(lspeed,rspeed);
//    delay(2);
//  }
  lspeed = 300;
  rspeed = 300;
  motors.setSpeeds(lspeed,rspeed);
//  delay(100);
}

void go_reverse() // move back
{
//  while (lspeed > -200 || rspeed > -200)
//  {
//    if (lspeed > -200)
//      lspeed -= 100;
//    else if (rspeed > -200)
//      rspeed -= 100;
//    motors.setSpeeds(lspeed,rspeed);
//    delay(2);
//  }
  lspeed = -300;
  rspeed = -300;
  motors.setSpeeds(lspeed,rspeed);
//  delay(100);
}

void  go_left()
{
  lspeed = -200;
  rspeed = 150;
  motors.setSpeeds(lspeed,rspeed);
//  lspeed = 0;
//  rspeed = 0;
//  delay(100);
}

void  go_right()
{
  lspeed = 150;
  rspeed = -200;
  motors.setSpeeds(lspeed,rspeed);
//  lspeed = 0;
//  rspeed = 0;
//  delay(100);
}

void go_still()
{
  lspeed = 0;
  rspeed = 0;
  motors.setSpeeds(lspeed,rspeed);
}

//================ END ====================
