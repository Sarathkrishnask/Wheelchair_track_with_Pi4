import RPi.GPIO as GPIO
import time
import serial
import numpy as np
import os
import subprocess
# from mpu6050 import mpu6050
import time
# mpu = mpu6050(0x68)
# Wheelchair class
class WheelChair(object):
    
    def __init__(self, lWheel, rWheel, maxWheelTeeth,digital_in):
        self._lWheelPins = (lWheel[0], lWheel[1])
        self._rWheelPins = (rWheel[0], rWheel[1])
        self._maxWheelTeeth = maxWheelTeeth
        self._digitalpin = digital_in
        self._connected = False
        self._digitalpin = False
        self.r_prev_angle=self.l_prev_angle=0
        self.dist_rWheel=self.dist_lWheel=0
        self.ang_l=self.ang_r=0
        self._delta_Teta=0
        self.d_baseline= 1.853675
        self.dcentre = self.phi=0
        self.X_=self.Y_=self.prev_X=self.prev_Y=0
        self.rad=self.deg=self.chng_teta=0
        while self._connected is False:
            try:
                self._ser = serial.Serial('/dev/rfcomm0')
                self._connected = True
            except:
                print("Trying to connect.")
#                 subprocess.run(["sudo rfcomm watch hci0"], shell = True)
                time.sleep(1)


 

        
        # Init wheel readers
        self.init_wheelreader()
        
        # Left and right wheel angles
        self._lWheelCount = 0
        self._rWheelCount = 0
        
        # Left and right angular velocity
        self._lWheelVel = 0
        self._rWheelVel = 0
        
    
    def init_wheelreader(self):
        # Left wheel
        GPIO.setmode(GPIO.BOARD)
        GPIO.setup(self._lWheelPins[0], GPIO.IN)
        GPIO.setup(self._lWheelPins[1], GPIO.IN)
        GPIO.setup(self._rWheelPins[0], GPIO.IN)
        GPIO.setup(self._rWheelPins[1], GPIO.IN)
        GPIO.setup(self._digitalpin, GPIO.OUT)
        # Assign callbacks
        GPIO.add_event_detect(self._lWheelPins[0], GPIO.RISING,
                              callback=self.leftWheelChange0, bouncetime=3)
        GPIO.add_event_detect(self._lWheelPins[1], GPIO.RISING,
                              callback=self.leftWheelChange1, bouncetime=3)
        GPIO.add_event_detect(self._rWheelPins[0], GPIO.RISING,
                              callback=self.rightWheelChange0, bouncetime=3)
        GPIO.add_event_detect(self._rWheelPins[1], GPIO.RISING,
                              callback=self.rightWheelChange1, bouncetime=3)
        
    
    def leftWheelChange0(self, channel):
        if (GPIO.input(self._lWheelPins[0]) == True and
            GPIO.input(self._lWheelPins[1]) == False):
            self._lWheelCount += 8.58
            self.dist_lWheel = ((self._lWheelCount - self.l_prev_angle) * 0.018892766)
#             self.l_prev_angle=self._lWheelCount
            self.data()
            if GPIO.input(self._lWheelPins[0]) == True:
                self.leftWheelChange1(channel)
              

    def leftWheelChange1(self, channel):    
        if (GPIO.input(self._lWheelPins[0]) == False and
            GPIO.input(self._lWheelPins[1]) == True):
            self._lWheelCount -= 8.58
            self.dist_lWheel = ((self._lWheelCount - self.l_prev_angle) * 0.018892766)
#             self.l_prev_angle=self._lWheelCount
            self.data()
            if GPIO.input(self._lWheelPins[1])== True:
                self.leftWheelChange0(channel)
                
    def rightWheelChange0(self, channel):
        if (GPIO.input(self._rWheelPins[0]) == True and
            GPIO.input(self._rWheelPins[1]) == False):
            self._rWheelCount += 8.58
            self.dist_rWheel += ((self._rWheelCount - self.r_prev_angle) * 0.018892766)#             ( pi/180)=0.01744 * 33 * 0.0328084 for ft convertion 
#             self.r_prev_angle = self._rWheelCount
            self.data()
            if GPIO.input(self._rWheelPins[0]) == True:
                self.rightWheelChange1(channel)
              

    def rightWheelChange1(self, channel):    
        if (GPIO.input(self._rWheelPins[0]) == False and
            GPIO.input(self._rWheelPins[1]) == True):
            self._rWheelCount -= 8.58
            self.dist_rWheel += ((self._rWheelCount - self.r_prev_angle) * 0.018892766)#             ( pi/180)=0.01744 * 33 * 0.0328084 for ft convertion 
#             self.r_prev_angle = self._rWheelCount
            self.data()
            if GPIO.input(self._rWheelPins[1])== True:
                self.rightWheelChange0(channel)
#                 
    def data(self):
        self.dcentre=((self.dist_rWheel + self.dist_lWheel) / 2)
        self.phi= ((self.dist_rWheel - self.dist_lWheel) / self.d_baseline)
        self._delta_Teta+= self.phi
        self.X_ += (self.dcentre * (math.cos(self._delta_Teta)))
        self.Y_ += (self.dcentre * (math.sin(self._delta_Teta)))
#         self.rad = math.atan2(self.X_,self.Y_)
#         self.deg = (self.rad * (180 / math.pi))
        self.chng_teta += (57.3 * self._delta_Teta)
        self.r_prev_angle = self._rWheelCount
        self.l_prev_angle = self._lWheelCount
# #         self.prev_X=self.X_
# #         self.prev_Y=self.Y_
    
    def start(self):
        while True:
            
#             if(self._ser.isOpen)== True:
#                 self._ser = serial.Serial('/dev/rfcomm0')
# 
#             val = f"{self._lWheelCount,self._rWheelCount}"
#             self.accel_data = mpu.get_accel_data()
#             self.Imu_data=round(self.accel_data['z'])         
#             val = f"{self._lWheelCount,self._rWheelCount,self.Imu_data}"
#             self._ser.write(bytearray(f"{val}\n".encode()))
#             print(val)
            swt = False
#         while True:

            if (GPIO.input(self._digitalpin) == True):
#                 self._digitalpin = True
                print(val)
                swt = True
            if swt == True:
                self.accel_data = mpu.get_accel_data()
                self.Imu_data=round(self.accel_data['z'])         
                val = f"{self._lWheelCount,self._rWheelCount,self.Imu_data}"
                self._ser.write(bytearray(f"{val}\n".encode()))
                print(val)
                val = f"{self.dist_rWheel,self.dist_lWheel,self._delta_Teta,self.X_,self.Y_,self.chng_teta}"
#                 self._digitalpin = True
                print(val)
            
            else:
                print("False")
        
#             else:
#                 subprocess.run(["sudo rfcomm watch hci0"],shell=True)
#                 print("nil")
        

                
#             self._ser = serial.Serial('/dev/rfcomm0')
           
            #self._ser.write(11)
#             self._ser.write(bytearray(f"{self._lWheelCount}\n{self._rWheelCount}\n".encode()))
#           print(bytearray(f"{self._lWheelCount,}\n".encode()))
#             print(self._ser.write(bytearray(f"{self._lWheelCount}\n".encode())))    
            time.sleep(0.1)


if __name__ == "__main__":
    leftWheelPins = (12, 11)
    rightWheelPins = (7, 8)
    maxWheelTeeth = 58
    digital_in = 11

#     subprocess.run("sudo rfcomm watch hci0", shell=True)
#     time.sleep(10)    
    wchair = WheelChair(leftWheelPins,rightWheelPins, maxWheelTeeth,digital_in)
#     wchair = WheelChair(rightWheelPins, maxWheelTeeth)
    wchair.start()




