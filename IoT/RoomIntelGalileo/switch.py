import urllib3
import mraa
import paho.mqtt.client as mqtt



class MQTTSub():
    def __init__(self, host, topic, topic2, message_handler, port, port2):
        self.__host = host
        self.__topic = topic
		self.__topic2 = topic2
        self.__handle = message_handler
        self.__connected = False

        def on_connect(client, userdata, flags, rc):
            if rc == 0:
                print("Connected successfully.")
                self.__connected = True
                client.subscribe(self.__topic)
				client.subscribe(self.__topic2)
            else:
                print("Server refuse connection with code %d", rc)

        def on_disconnect(client, userdata, rc):
                print("Lost connection to MQTT broker, trying to repair")
                client.reconnect()

        def on_message(client, userdata, msg):
            self.__handle(msg.payload, port, port2)

        self.__client = mqtt.Client(protocol=mqtt.MQTTv31)
        self.__client.on_connect = on_connect
        self.__client.on_disconnect = on_disconnect
        self.__client.on_message = on_message
        
        self.__client.connect(self.__host)

    def LoopForever(self):
		self.__client.loop_forever()
		

    def LoopStart(self):
        self.__client.loop_start()

    def LoopStop(self):
        self.__client.loop_stop(force=False)



def ack_value(did, value, port):
    switch_on_off(value, port)
    URL_BASE = "http://168.63.82.20/server/income/?did=%s&action=ack&value=" % did
    url = URL_BASE + value
    try:
    	 urllib3.PoolManager().urlopen('GET', url,  preload_content=False)
    except urllib3.exceptions.HTTPError as e:
    	if e.code == '404':
    		print("URL not found: %s" % url)



DID = '190pfxztlgyu'
DID2 = 'nck9b5j1ao9k'

# How to use:
#
# 1. write the handler function. 
# It'll be invoke than new message arrive on topic
def switch_on_off(state, port):
    x = mraa.Gpio(port)
    x.dir(mraa.DIR_OUT)
    x.write(int(state))

def handler(str_msg, port, port2):
    msg_dict = dict()
    msg_list = str_msg.split(";")
	print (str_msg)
    for item in msg_list:
        tmp = item.split("=")
        if len(tmp) == 2:
            msg_dict[tmp[0]] = tmp[1]
        else:
            print("Error in income msg: %s" % str_msg)
            return

    if not msg_dict.has_key('did'):
        print("No DID in income msg: %s" % str_msg)
        return
    if not msg_dict.has_key('action'):
        print("No action in income msg: %s" % str_msg)
        return

    if msg_dict['action'] == 'set':
        if not msg_dict.has_key('value'):
            print("No new value in income msg: %s" % str_msg)
            return
        if msg_dict['value'] == "1":
            # code for setting switch on
            if msg_dict['did'] == DID:
				ack_value(msg_dict['did'], "1", port)
			else:
				ack_value(msg_dict['did'], "1", port2)
            return
        elif msg_dict['value'] == "0":
            # code for setting switch off
			if msg_dict['did'] == DID:
				ack_value(msg_dict['did'], "0", port)
			else:
				ack_value(msg_dict['did'], "0", port2)
            return
        else:
            print("Unknown value: %s" % str_msg)
            return
    elif msg_dict['action'] == 'get':
        # return current value of switch need to be stored somethere
        # ack_value(DID, value, port) 
        return
    else:
        print("Unknown action: %s" % str_ms)

# 2. Create exemplar of MQTTSub class.
# For initialization provide MQTT brocker adress, 
# needed topic and handler function


mqtt_con = MQTTSub("168.63.82.20", "sbst/thing/" + DID, "sbst/thing/" + DID2, handler, 8, 7)

# 3. Start MQTT client loop.
mqtt_con.LoopForever()