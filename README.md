This project implements a client-server note-sharing system in C#. It supports secure user authentication, note encryption, and structured request/response handling over HTTP.

![{E8C1F7CC-2B49-41F6-923B-26AB9F9A8F2B}](https://github.com/user-attachments/assets/6f87dd56-a77c-4414-8fc3-82fe12139a02)

**SERVER-SIDE**

**Server.cs (ClientHandling):**
Handles client connections and delegates request processing.

* `ProcessRequests`:

  * Starts an `HttpListener` and continuously listens for incoming connections until the cancellation token is set.
  * Each new or known client is associated with a `ClientInterface` instance stored in `ClientCollection`.
  * Incoming requests are passed to the appropriate `ClientInterface` for processing.
* `Start`:

  * Launches a new thread that runs `ProcessRequests`.
* `Stop`:

  * Sets the cancellation token to terminate the server thread gracefully.
* `~Server()`:

  * Ensures cleanup by calling `Stop`.

**ClientInterface.cs (ClientHandling):**
Manages a single client's session and processes its requests.

* Stores client identification (IP or email) and session data.
* `DispatchResponse`: Sends serialized `ServerResponse` to the client.
* `DispatchError`: Sends error messages using `DispatchResponse`.
* `GetClientRequest`: Parses the JSON string from POST requests into `ClientRequest` objects.
* `Process`:

  * Executed for every client request (only POSTs).
  * Deserializes `ClientRequest`, identifies the handler using a switch statement, and returns a `ServerResponse`.

**RequestHandler.cs (ClientHandling):**
Static class that processes all request types.

* Maps request type to handler logic.
* Uses `DatabaseHandler` for any operations requiring persistent data.

**DatabaseHandler.cs (Database):**
Static class for interacting with SQLite storage.

* Uses a local SQLite file `JournalDB.db`.
* Performs user auth, signup, and note storage/retrieval.
* Implements parameterized SQL to prevent SQL injection.

**Logger.cs (Logging):**
Static logging utility.

* Appends `Log` entries that describe server events.
* Includes helper methods like `AppendMessage`.

**\[Log/DebugLog/ErrorLog/WarnLog].cs (Logging):**
Represent different types of server log events.

* Store message content and timestamp.

---

**CLIENT-SIDE**

**Client.cs (ClientSide):**
Represents a single client that interacts with the server.

* Uses `HttpClient` to send and receive requests.
* `SendRequest`:

  * Serializes `ClientRequest`, sends it via POST, and deserializes the `ServerResponse`.
* `SendContent`:

  * Low-level method used by `SendRequest` for HTTP exchange.
* `ReceiveRequest`:

  * Used for earlier HTTP GET testing.
* High-level methods (`SignUp`, `PostNote`, `GetNote`, etc):

  * Construct appropriate request objects.
  * Use `SendRequest` internally.
  * Handle encryption keys and local note storage in `/Notes`.

---

**COMMON (Shared Server/Client Code)**

**CommunicationObject.cs (Containers):**
Base class for serializable request/response objects.

* Not instantiated directly.
* Includes `Body` (a string holding serialized object data).
* `Serialise`: Converts the entire object into a JSON string.

**ClientRequest.cs (Containers):**
Extends `CommunicationObject`.

* `ClientRequestType`: Enum indicating the request type.
* `TryGetLoginDetails`: Attempts to deserialize `Body` into a `LoginDetails` object and returns success status.
* Used for all POST requests from client to server.

**ServerResponse.cs (Containers):**
Extends `CommunicationObject`.

* `ServerResponseType`: Enum indicating the response type.
* `Body`: Contains the JSON representation of a `Note` or debug message.

**LoginDetails.cs (Containers):**
Handles credentials and authentication hash generation.

* Takes plaintext email and password.
* Uses `PasswordHashing` to:

  * Derive an encryption key for note protection.
  * Generate a base64-encoded authentication hash for login/signup.
* Stores the encryption key in an output argument for later use by the client.

**Note.cs (Containers):**
Encrypted representation of a user's note.

* Serialized and saved locally as JSON in `/Notes`.
* `Title`: Plaintext (used as filename and visual identifier).
* `InternalData`: Encrypted content.
* `InitVector` and `SecurityTag`: Used for AES-GCM decryption and verification.
* `SetText`: Encrypts and stores new note content.
* `GetText`: Decrypts and returns the stored content.
* `Delete`: Removes the corresponding note file.

**ClientRequestTypes.cs / ServerResponseTypes.cs (Types):**
Enum definitions for identifying request and response types.

**PasswordHashing.cs (Security):**
Handles all cryptographic hashing operations.

* `DeriveHash`: Core function for generating secure hash values.

  * For encryption key:

    * `plaintextBytes` = password
    * `saltBytes` = email
    * Prevents rainbow table attacks by salting with unique emails.
  * For authentication hash:

    * `plaintextBytes` = note encryption key
    * `saltBytes` = password
    * Enhances security by using the user's secret password as salt.
* Server-side:

  * Also used during signup/login for generating/storing/revalidating hashes.
  * Uses randomly generated salt per user (stored with the hash in the database).
* Includes `CompareAuthHash` for secure equality check of stored vs received hashes.

---

**Network Interactions:**
All communication is handled over HTTP using the `System.Net` library.

* Clients use `HttpClient` to send serialized `ClientRequest` objects via POST.
* Server receives the request, processes it, and responds with a serialized `ServerResponse`.
* Only POST is used in live operation.
* All note and credential-related data is encrypted on the client side before transmission.
