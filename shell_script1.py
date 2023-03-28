import subprocess
import time
# from mpu6050 import mpu6050

subprocess.Popen(["sudo rfcomm watch hci0"], shell=True)
subprocess.Popen(["python3 /home/pi/Desktop/sarath/wheel_chair_v2.py"], shell=True)

# import time
# def val():
#         
#     while True:
#                 try:
#                     pass
# #                     print("trying to connect")
# # #                     time.sleep(1)
# 
#     #                 self._ser = serial.Serial('/dev/rfcomm0')
#     #                 self._connected=True
#                 except:
#                     print("Trying to connect.")
#     #                 subprocess.run(["sudo rfcomm watch hci0"], shell = True)
# 
# if __name__ == "__main__":
#     val()