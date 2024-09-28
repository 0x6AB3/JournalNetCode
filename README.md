SERVER-SIDE
Server.cs (ClientHandling):
	This class handles connecting clients in 'ProcessRequests':
	The 'HttpListener' object listens to connecting clients while the thread cancel token is not set
	The 'ClientInterface' class is instantiated for each unique client
	Existing clients are located within the 'ClientCollection' structure
	The request is passed to and processed by the new/corresponding 'ClientInterface' object
		The 'Start' method begins a new thread that executes 'ProcessRequests'
		The 'Stop' method requests the 'ProcessRequests' thread to end by setting the 'CancelToken' object
		The destructor executes the 'Stop' method
	
ClientInterface.cs (ClientHandling):
	This class stores the client identifier/s and a public method for their retrieval (endpoint if logged out / email if logged in)
	This class stores the client session and contains methods for client interaction:
		'DispatchResponse' is the main method for sending data to the client
		'DispatchError' method uses the 'DispatchResponse' method and is used for invalid requests
		'GetClientRequest' retrieves the 'ClientRequest' object from the JSON string that comes with each POST request
	The main 'Process' method is executed for each client request:
		Only 'POST' requests are sent by the client and received by the server (Server <-- RX -- Client request)
			The server expects each 'POST' request to contain a 'ClientRequest' JSON string
		The server sends back a 'ServerResponse' object containing the processed request output (Server response -- TX --> Client)
		A switch case is employed to identify the corresponding 'RequestHandler' method to which the request object is passed
			A 'ServerResponse' object is returned and sent to the client using 'DispatchResponse'
	
RequestHandler.cs (ClientHandling):
	This is a static class that processes all client requests and returns the corresponding 'ServerResponse' object
	The 'DatabaseHandler' class is utilised when database operations are required
		(account creation/lookup and note upload/download)
	
DatabaseHandler.cs (Database):
	This is a static class that contains all defined interactions with the database
	The database is stored in a single 'JournalDB.db' SQLite file (this would be changed for a larger user base)
	Parameterised SQL is used to prevent SQL injection attacks
		
Logger.cs (Logging):
	This is a static class that stores a list of 'Log' objects that contain information about the server (such as client interactions)
	Helper methods exist (such as 'AppendMessage') for ease of logging

[Log/DebugLog/ErrorLog/WarnLog].cs (Logging)
	Objects of this type represent a server event at a given time
	These classes contain message and time properties of the given event
		
		
CLIENT-SIDE
Client.cs (ClientSide):
	This class represent a client connection to the server and provides methods for server interaction (login, signup, note upload, etc)
	The session is contained within the HttpClient property
	The 'SendRequest' method sends 'ClientRequest' objects and receives 'ServerResponse' objects
		This method is used by all higher-level methods (such as 'SignUp', 'PostNote', etc)
		The request is serialised and sent to the server through the HTTP protocol using 'POST'
			The response is deserialised into a 'ServerResponse' object and returned
			If the request execution is successful and the request was 'GetNote', then the retrieved note is deserialised
			and stored on the local device in '/Notes' in the executable file directory
	The 'SendContent' method is utilised here to send the serialised request and receive the serialised response
	The 'ReceiveRequest' method was used previously for HttpClient testing
		HTTP 'GET' is used here to retrieve content from the server
	The necessary 'LoginDetails' (for 'SignUp'/'LogIn') and 'Note' (for 'PostNote'/'GetNote') objects are instantiated when required in higher-level methods
		The email and password arguments in the given method are passed to the 'LoginDetails' constructor 
			(such as the email/password string or note object)
	The Higher-level methods (LogIn, SignUp, GetNoteTitles, etc) return a 'ServerResponse' object that contains the processed request

COMMON (Both [server/client]-side)
CommunicationObject.cs (Containers):
	This class is a container for all other serialised classes in the Containers namespace (inside of the 'Body' string)
	The 'Serialise' method creates a JSON string from the CommunicationObject itself so that it may be sent as one JSON string 
		(attached to the POST request)
	This class is never instantiated itself but rather inherited by 'ClientRequest' and 'ServerResponse'
		The JSON string representations of 'LoginDetails' and 'Note' are stored inside of the 'Body' property
	
ClientRequest.cs (Containers ):
	This class inherits 'CommunicationObject' and includes a 'ClientRequestType' property that specifies the request
	The 'TryGetLoginDetails' method attempts to deserialise the 'Body' string property into a 'LoginDetails' object,
	returning a Boolean value of 'true'/'false' as the method return type
		The deserialised 'LoginDetails' object is also assigned to the out argument (null if method returns 'false')
	Objects of this type are instantiated and serialised by the client and attached to the HTTP 'POST' request which is deserialised
	by the server and processed
	The 'Body' property (inherited from 'CommunicationObject') is used for requests of type 'SignUp'/'LogIn' (set to the JSON string
		representation of 'LoginDetails') and 'PostNote' (set to the JSON string representation of the corresponding 'Note' object)
	
ServerResponse.cs (Containers ):
	This class inherits 'CommunicationObject' and includes a 'ServerResponseType' property that specifies the response to a processed/invalid request
	Objects of this type are instantiated and serialised by the server and deserialised by the client
	The 'Body' property is set to the 'Note' object JSON string for ClientRequestType 'GetNote'
		For actions that don't require an object to be retrieved, an output message is set instead (for debugging and logging)

LoginDetails.cs (Containers):
	This class contains all of the information required by the server for a client to signup/login (email and authentication hash)
	The main constructor takes the plaintext email and password strings and feeds them to the 'PasswordHashing' object to
	generate an encryption key (used for encrypting Note object contents) which is then fed to the hashing algorithm to generate 
	an authentication hash (sent to the server for authentication)
	The generated authentication hash is stored as a base64 string (so that the entire 'LoginDetails' object may be serialised)
	The encryption key out argument is set to the generated encryption key so that the 'Client' class may reuse it
		
Note.cs (Containers):
	This class is instantiated for each note that the user creates
	Note files are serialised and stored as a JSON string inside of the '/Notes' directory with their title as the filename
	The AES-GCM stream cypher is used for encryption/decryption with 256-bit keys
	The 'Title' string property is plaintext as it is used for note identification
		Encrypted titles would increase the time taken to display all notes in the client-side software
		Titles are also unlikely to hold sensitive content, rather they are more likely to identify the files in which the sensitive content is contained
	'InternalData' contains the encrypted note content (the 'Body' of the note, where private information is likely to be held)
	The initialisation vector ('InitVector') and security tag ('SecurityTag') properties are unique for each 'SetText' operation and are used for note content decryption and authentication
	The 'SetText' method takes the plaintext argument and encrypts it using the encryption key argument
		The 'Body' of the note is temporarily stored and displayed as plaintext in the client-side software, inside of a textbox field
	The 'GetText' method decrypts 'InternalData' with the encryption key argument and returns the plaintext
	The 'Delete' method removes the file with the 'Title'.json filename from the '/Notes' directory
	
[ClientRequestTypes/ServerResponseTypes].cs (Types):
	Both files contain an enum structure for the corresponding 'ClientRequest'/'ServerResponse' class 'RequestType'/'ResponseType'
		
PasswordHashing.cs (Security):
	This class is used to generate the client encryption key (for note encryption/decryption) and authentication hash (server authentication; signup/login)
	The iteration count is determined by the constructor argument that sets the '_iterations' integer property
	'DeriveHash' is the core method for calculating the hash bytes for the given password and salt byte arrays
		For encryption key derivation:
			The 'plaintextBytes' argument is set to the user's password
				This is the sensitive information that is kept private
			The 'saltBytes' argument is set to the user's email
				Each user has a unique email address, resulting in a unique salt for each account
				This prevents precomputed rainbow table attacks because an attacker would need to know all user emails that are stored in the database and then
					iterate through a wordlist for each account with that account's unique salt, further complicating their attempts
		For authentication hash derivation:
			The 'plaintextBytes' argument is set to the user note encryption key
				This is information is only temporarily stored on the client side in the main memory during runtime and never transmitted
			The 'saltBytes' argument is set to the user's password
				This information is secret and serves as a unique salt for each user
				An attacker would be required to know the user password beforehand to calculate the authentication hash for themselves
		Server-side use:
		'DeriveHash' is used for a third time by the server during account signup and login in 'DatabaseHandler' to reinforce the stored hash further
			A pseudorandom salt is generated and stored alongside the generated authentication hash in the 'tblAccounts' table inside of the SQLite database
	The server executes the 'CompareAuthHash' method to authenticate the user by comparing their authentication hash to the stored hash and returning the result

Network interactions:
	All network interactions are carried out over HTTP through the use of the System.Net library
	Clients connect to the server with the 'HttpClient' object inside of 'Client.cs'
