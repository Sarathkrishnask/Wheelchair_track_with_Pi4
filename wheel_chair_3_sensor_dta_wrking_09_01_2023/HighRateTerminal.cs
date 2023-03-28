using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TechTweaking.Bluetooth;
using UnityEngine.UI;
using System;
using CodeMonkey.Utils;


public class HighRateTerminal : MonoBehaviour
{
	
	public Text devicNameText;
	public ScrollTerminalUI readDataText;//ScrollTerminalUI is a script used to control the ScrollView text
	public string content;
	public GameObject InfoCanvas;
	public GameObject DataCanvas;
	private  BluetoothDevice device;
	public Text dataToSend;
	public Text ang1, ang,Imu_data1,count_1;
	public Text len, XDir, YDir, The_Dir;
	public float val, Xx_ =0.0f;
	public Rigidbody2D rb;
	public float speed = 1.1f;
	private Vector2 moveDirection;
	public GameObject Image, Image1,car;
	public static float encL, encR, Langle, Rangle,Lradian,Rradian,Imu;			  //encL, encR are represent the wheelchair left &  rigth wheel
	public static float Dcentre;			  //Dcenter is the average of distance travelled by wheel
/*	public static int Wdia = 6;				  //Diameter of Wheel
	public static int Wrad = 1;				  //Radius of Wheel*/
	public static float Dbaseline = 1.792f;          //Distance between the two wheel chair wheels
	public static float Theta, Theta_, Phi,Ldist,Rdist;                   //Rate of change of new postion of angle//
	public static float X, X_, Y, Y_, X_Dir, Y_Dir, Theta_dir;
	public float prevLang, prevRang;
	public static int count=0;

	// plotting function variable decleartion:
	[SerializeField] private Sprite circleSprite;
	public RectTransform graphContainer;
	public RectTransform labelTemplateX;
	public RectTransform labelTemplateY;
	private RectTransform dashTemplateX;
	private RectTransform dashTemplateY;
	private List<GameObject> gameObjectList;
	private int maxVisibleValueAmount;
	public List<int> valueList = new List<int>() { 5, 16, 25, 4, 45, 60, 76, 87, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 77, 34, 34, 5, 6, 9, 8, 65 };

	/*	public bool satOnce;
	*/
	/*	public string value;
	*/
	void Awake ()
	{

		// bluetooth plot decleration

		//graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
		labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
		labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
		dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
		dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
		gameObjectList = new List<GameObject>();

		BluetoothAdapter.askEnableBluetooth ();//Ask user to enable Bluetooth
		
		BluetoothAdapter.OnDeviceOFF += HandleOnDeviceOff;
		BluetoothAdapter.OnDevicePicked += HandleOnDevicePicked; //To get what device the user picked out of the devices list




       /* List<int> valueList = new List<int>() { 5, 16, 25, 4, 45, 60, 76, 87, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 77, 34, 34, 5, 6, 9, 8, 65 };*/
        /*List<int> valueList = new List<int>() {1,2,3,4,3,2,1,2,3,4,3,2,1,2,3,4,3,2,1,0 };*//*
		ShowGraph(valueList, -1);

        FunctionPeriodic.Create(() =>
        {
            valueList.Clear();
            for (int i = 0; i < 22; i++)
            {
                valueList.Add(UnityEngine.Random.Range(0, 500));
            }
            ShowGraph(valueList);
        }, .5f);*/

    }

    void HandleOnDeviceOff (BluetoothDevice dev)
	{
		if (!string.IsNullOrEmpty (dev.Name))
			devicNameText.text = "Can't connect to " + dev.Name + ", device is OFF";
		else if (!string.IsNullOrEmpty (dev.Name)) {
			devicNameText.text = "Can't connect to " + dev.MacAddress + ", device is OFF";
		}
	}
	
	//############### UI BUTTONS RELATED METHODS #####################
	public void showDevices ()
	{
		Debug.Log("asmkfnkas");
		BluetoothAdapter.showDevices ();//show a list of all devices//any picked device will be sent to this.HandleOnDevicePicked()
		//car.SetActive(false);
	}
	
	public void connect ()//Connect to the public global variable "device" if it's not null.
	{
		if (device != null) {
			device.connect ();
			//car.SetActive(true);
		}
	}
	
	public void disconnect ()//Disconnect the public global variable "device" if it's not null.
	{
		if (device != null)
			device.close ();
	}
	
	public void send ()
	{		
		if (device != null && !string.IsNullOrEmpty (dataToSend.text)) {
			device.send (System.Text.Encoding.ASCII.GetBytes (dataToSend.text + (char)10));//10 is our seperator Byte (sepration between packets)
		}
	}
	
	void HandleOnDevicePicked (BluetoothDevice device)//Called when device is Picked by user
	{
		
		this.device = device;//save a global reference to the device
		
		//this.device.UUID = UUID; //This is only required for Android to Android connection
		
		/* 
		 * setEndByte(10) will change how the read() method works.
		 * 10 equals the char '\n' which is a "new Line" in Ascci representation, 
		 * so the read() method will retun a packet that was ended by the byte 10, without including 10.
		 * Which means read() will read lines while excluding the '\n' new line charachter.
		 * If you don't use the setEndByte() method, device.read() will return any available data (line or not), then you can order/packatize them as you want.
		 * 
		 * Note: setEndByte will make reading lines or packest easier.
		 */
		device.setEndByte (10);
		
		
		//Assign the 'Coroutine' that will handle your reading Functionality, this will improve your code style
		//Other way would be listening to the event Bt.OnReadingStarted, and starting the courotine from there
		device.ReadingCoroutine = ManageConnection;
		
		devicNameText.text = device.Name;
		
	}
    public void Reset()
    {
		Theta_ = 0;
		val = 0.0f;
		X_ = 0;
		Y_ = 0;
		count = 0;
		car.transform.position = new Vector3(X_Dir, Y_Dir, 0);
		car.transform.rotation = Quaternion.Euler(0, 0, 57.3f * Theta_);

	}
    private void Update()
	{
        /*		if (Imu < -2)
				{
					Imu_data1.text = "SITTING";
		*//*	 satOnce = true;
		*//*

		}
				else
					Imu_data1.text = "STANDING";
					*//*if(satOnce)
					count = count + 1;
					satOnce = false;*//*

				count_1.text = count.ToString();
				//Imu_data1.text = Imu.ToString();
				ang.text = encL.ToString();
				ang1.text = encR.ToString();
				*//* XDir.text = Lradian.ToString();
				 YDir.text = Rradian.ToString();*//*
				XDir.text = X_.ToString();
				YDir.text = Y_.ToString();
				The_Dir.text = (57.3f * Theta_).ToString();
				//car.SetActive(true);
				car.transform.position = new Vector3(X_Dir, Y_Dir, 0);
				car.transform.rotation = Quaternion.Euler(0, 0, -57.3f* Theta_ + 90);*/


        //ang1 = rEnc.ToString();



       /* List<int> valueList = new List<int>() { 5, 16, 25, 4, 45, 60, 76, 87, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 77, 34, 34, 5, 6, 9, 8, 65 };*/
        /*List<int> valueList = new List<int>() {1,2,3,4,3,2,1,2,3,4,3,2,1,2,3,4,3,2,1,0 };*/
        /*ShowGraph(valueList, -1);*/
        Debug.Log("data");
		/*Debug.Log(len.text);*/
		/*Debug.Log(XDir.text);
		Debug.Log(content);*/
		/*ShowGraph(valueList,-1);*/
	/*	FunctionPeriodic.Create(() =>
        {
            valueList.Clear();
            for (int i = 0; i < 15; i++)
            {
                valueList.Add(UnityEngine.Random.Range(0, 200));
            }
            ShowGraph(valueList);
        }, .0f);*/


    }

	//public void dataset()
   // {

/*		Rdist = (Rangle - prevRang) * 6.79f / 360; // circumference of wheel is 81.5inch => 6.79 feet
		Ldist = (Langle - prevLang) * 6.79f / 360;  //distance langle*6.28/360;
		Dcentre = (Ldist + Rdist) / 2;
		Phi = (Rdist - Ldist )/ Dbaseline;  //3.28 for convert to ft to mtr
		Theta_ += Phi; //to convert angle to radian
					   // constrain _theta to the range 0 to 2 pi
		//if (Theta_ > 2.0 * Mathf.PI) Theta_ -= 2.0f * Mathf.PI;
		//if (Theta_ < 0.0) Theta_ += 2.0f * Mathf.PI;

		Y_ += (Dcentre * (float)(Math.Cos(Theta_)));
		X_ += (Dcentre * (float)(Math.Sin(Theta_)));


		X_Dir = X_ * 0.2f;
		Y_Dir = Y_ * 0.2f;

		prevLang = Langle;
		prevRang = Rangle;*/
		//Theta = Theta_;


		/*     var charsToRemove = new string[] { "@", ",", ".", ";", "'", "(", ")" };

			 foreach (var c in charsToRemove)
			 {
				 leng.text = content.Replace(c, string.Empty);

			 }*/


	//}

    //############### Reading Data  #####################
    //Please note that you don't have to use Couroutienes, you can just put your code in the Update() method
    //If you want to achieve a minimum delay please check the "High Bit Rate Terminal" demo
    IEnumerator  ManageConnection (BluetoothDevice device)
	{//Manage Reading Coroutine
		
		//Switch to Terminal View
		InfoCanvas.SetActive (false);
		DataCanvas.SetActive (true);
		
		
		
		while (device.IsReading) {
			
			//polll all available packets
			BtPackets packets = device.readAllPackets ();
			
			
			if (packets != null) {
				
				/*
				 * parse packets, packets are ordered by indecies (0,1,2,3 ... N),
				 * where Nth packet is the latest packet and 0th is the oldest/first arrived packet.
				 * 
				 */
				
				for (int i=0; i<packets.Count; i++) {
					Debug.Log("hi");
					//packets.Buffer contains all the needed packets plus a header of meta data (indecies and sizes) 
					//To parse a packet we need the INDEX and SIZE of that packet.
					int indx = packets.get_packet_offset_index (i);
					int size = packets.get_packet_size (i);
					
					string content = System.Text.ASCIIEncoding.ASCII.GetString (packets.Buffer, indx, size);
					/*len.text = packets.Buffer[0].ToString();
					XDir.text = packets.Buffer[1].ToString();*/
					//Debug.Log(content.ToString());
					/*YDir.text = packets.Buffer[2].ToString();*/

					Debug.Log(content.ToString());
					string[] lines = content.Split(new char[] { '\n', '\r', '(', ')',',' });
					Debug.Log(lines[0].ToString());
					Debug.Log(lines[1].ToString());
					val = float.Parse(lines[0]);
					//Debug.Log(lines[0].ToString());
					X_ = val;
					/*Y_ = float.Parse(lines[1].ToString());
					Theta_ = float.Parse(lines[2].ToString());*/
					if (X_ > prevLang)
                    {
						X_ += X_;
						YDir.text = X_.ToString();
						Xx_ = X_ * 0.01745f;
						XDir.text = Xx_.ToString();
						len.text = Theta_.ToString();
						prevLang = X_;

					}
					else if (X_ < prevLang)
                    {
						X_ -= X_;
						YDir.text = X_.ToString();
						Xx_ = X_ * 0.01745f;
						XDir.text = Xx_.ToString();
						len.text = Theta_.ToString();
						prevLang = X_;

					};

			



                  /*  string val = Image.GetComponent<Text>().text;
                    float movX = float.Parse(val);
                    string val1 = Image1.GetComponent<Text>().text;
                    float movY = float.Parse(val1);
                    moveDirection = new Vector2(movX, movY).normalized;
                    rb.velocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);*/

                    //readDataText.add (device.Name, content);
                }
			}
			
			
			yield return null;
		}
		
		//Switch to Menue View after reading stoped
		DataCanvas.SetActive (false);
		InfoCanvas.SetActive (true);	
	}
	
	
	//############### UnRegister Events  #####################
	void OnDestroy ()
	{
		BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked; 
		BluetoothAdapter.OnDeviceOFF -= HandleOnDeviceOff;
	}




	//// here we write the code for data plot from bluetooth receive
	///

	private void ShowGraph(List<int> valueList, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
	{
		if (getAxisLabelX == null)
		{
			getAxisLabelX = delegate (int _i) { return _i.ToString(); };
		}
		if (getAxisLabelY == null)
		{
			getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
		}
		if (maxVisibleValueAmount <= 0)
		{
			maxVisibleValueAmount = valueList.Count;
		}
		foreach (GameObject gameObject in gameObjectList)
		{
			Destroy(gameObject);
		}
		gameObjectList.Clear();


		float graphWidth = graphContainer.sizeDelta.x;
		float graphHeight = graphContainer.sizeDelta.y;
		/*float xSize = 50f;*/
		float xSize = graphWidth / (maxVisibleValueAmount);

		/*float yMaximum = 0f;*/
		float yMaximum = valueList[0];
		float yMinimum = valueList[0];
		/*   foreach (int value in valueList)
           {
               if (value > xSize)
               {
                   xSize = value;
               }
           }

           xSize = xSize * 1.2f;*/

		/*foreach (int value in valueList)*/
		for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
		{
			int value = valueList[i];
			if (value > yMaximum)
			{
				yMaximum = value;
			}

			if (value < yMinimum)
			{
				yMinimum = value;
			}
		}

		float yDifference = yMaximum - yMinimum;
		if (yDifference <= 0)
		{
			yDifference = 5f;
		}

		/*yMaximum = yMaximum * 1.2f;*/
		yMaximum = yMaximum + (yDifference * 0.2f);
		yMinimum = yMinimum - (yDifference * 0.2f);

		yMinimum = 0f;

		int xIndex = 0;

		GameObject lastCircleGameobject = null;
		for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
		{
			/*float xPosition = (xSize + i * xSize);*/
			float xPosition = (xSize + xIndex * xSize);
			/*float yPosition = (valueList[i] / yMaximum) * graphHeight;*/
			float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum) * graphHeight);
			GameObject circleGameobject = CreateCircle(new Vector2(xPosition, yPosition));
			gameObjectList.Add(circleGameobject);

			if (lastCircleGameobject != null)
			{
				GameObject dotConnectionGameObject = CreateDotConnection(lastCircleGameobject.GetComponent<RectTransform>().anchoredPosition, circleGameobject.GetComponent<RectTransform>().anchoredPosition);
				gameObjectList.Add(dotConnectionGameObject);
			}
			lastCircleGameobject = circleGameobject;

			RectTransform labelX = Instantiate(labelTemplateX);
			labelX.SetParent(graphContainer, false);
			labelX.gameObject.SetActive(true);
			labelX.anchoredPosition = new Vector2(xPosition, 60f);
			/*labelX.GetComponent<Text>().text = i.ToString();*/
			labelX.GetComponent<Text>().text = getAxisLabelX(i);
			gameObjectList.Add(labelX.gameObject);


			/*RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(graphContainer, false);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xPosition, -3f);
            gameObjectList.Add(dashX.gameObject);*/

			xIndex++;




		}
		int separatorCount = 10;
		for (int i = 0; i <= separatorCount; i++)
		{
			RectTransform labelY = Instantiate(labelTemplateY);
			labelY.SetParent(graphContainer, false);
			labelY.gameObject.SetActive(true);
			float normalizeValue = i * 1f / separatorCount;
			labelY.anchoredPosition = new Vector2(-7f, normalizeValue * graphHeight);
			labelY.GetComponent<Text>().text = Mathf.RoundToInt(normalizeValue * yMaximum).ToString();
			gameObjectList.Add(labelY.gameObject);

			/* RectTransform dashY = Instantiate(dashTemplateY);
			 dashY.SetParent(graphContainer, false);
			 dashY.gameObject.SetActive(true);
			 dashY.anchoredPosition = new Vector2(-4f, normalizeValue * graphHeight);
			 gameObjectList.Add(dashY.gameObject);*/

		}
	}

	private GameObject CreateCircle(Vector2 anchoredPosition)
	{
		GameObject gameObject = new GameObject("circle", typeof(Image));
		gameObject.transform.SetParent(graphContainer, false);
		gameObject.GetComponent<Image>().sprite = circleSprite;
		RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = new Vector2(12, 12);
		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(0, 0);
		return gameObject;
	}

	private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
	{
		GameObject gameObject = new GameObject("dotConnection", typeof(Image));
		gameObject.transform.SetParent(graphContainer, false);
		gameObject.GetComponent<Image>().color = new Color(0, 1, 0, .8f);
		RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
		Vector2 dir = (dotPositionB - dotPositionA).normalized;
		float distance = Vector2.Distance(dotPositionA, dotPositionB);
		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(0, 0);
		rectTransform.sizeDelta = new Vector2(distance, 4f);
		/* rectTransform.anchoredPosition = dotPositionA;*/
		rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
		rectTransform.localEulerAngles = new Vector3(0, 0, (Mathf.Atan2(dir.y, dir.x) * 180 / Mathf.PI)); /*(Mathf.Atan2(dir.y, dir.x)*180/Mathf.PI)*/
		return gameObject;

	}





}
