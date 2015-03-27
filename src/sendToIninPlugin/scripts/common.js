var inin_appname = "Hootsuite Integration";
var inin_credsCookie = 'ININHootsuiteCredsCookie';
var inin_sessionCookie = 'ININHootsuiteSessionCookie';
var inin_sessionCookieTimeout = 3;
var inin_messagePollInterval = 10000;
var inin_messagePollTimer;
var inin_closeCustomPopupTimer;
var inin_sessionId = null;
var inin_csrfToken = null;
var inin_server = '';
var inin_username = '';
var inin_password = '';
var inin_loginRedirect;
var inin_noSessionCallback;
var inin_pollingEnabled = false;

/****** IMMEDIATE SCRIPTS ******
 * These scripts are run as soon as this script file is loaded.
 */

// Ensure String.startsWith is a function
if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str){
        return this.slice(0, str.length) == str;
    };
}

// Ensure String.endsWith is a function
if (typeof String.prototype.endsWith != 'function') {
  String.prototype.endsWith = function (str){
    return this.slice(-str.length) == str;
  };
}



/****** GENERAL ******
 * These scripts provide general or multi-purpose functionality
 */

// Sets an error message in the UI
function setError(message) {
    if (message == undefined || message == '') {
        $('#inin-error').css('display', 'none');
        $('#inin-error').html('No error');
    } else {
        console.error(message);
        $('#inin-error').css('display', 'block');
        $('#inin-error').html(message);
    }
}

// Determines if the passed in value is a valid numeric value
function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

// Gets a querystring parameter value by name
function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

// Sends a request to an ICWS resource
function sendRequest(verb, resource, requestData, successCallback, errorCallback) {
    // Refresh session cookie
    var sessionCookie = $.cookie(inin_sessionCookie);
    if (sessionCookie != undefined) {
        // Set to expire in X minutes
        var expiryDate = new Date();
        expiryDate.setMinutes(expiryDate.getMinutes() + inin_sessionCookieTimeout);

        // Save cookie
        $.cookie(inin_sessionCookie, sessionCookie, { expires: expiryDate });
    }

    // Build request
    var request = {
        type: verb,
        success: successCallback,
        error: errorCallback,
        headers: {},
        dataFilter:dataFilter
    };

    // Enable CORS
    request.xhrFields = { withCredentials:'true' };

    // Set URL
    request.url = inin_server + '/icws/' + resource;

    // Add standard headers
    request.headers['Accept-Language'] = 'en-us';
    request.headers['Accept-Language'] = 'en-us';

    // Add CSRF token
    if (inin_csrfToken)
        request.headers['ININ-ICWS-CSRF-Token'] = inin_csrfToken;

    // Add data
    if (requestData)
        request.data = JSON.stringify(requestData);

    // Send request
    console.debug(request.type + ' ' + request.url);
    $.ajax(request);
}

// Ensures that ICWS responses have a JSON body for jQuery to parse
function dataFilter(data, type) {
    /****** KNOWN ISSUE ******
     * The purpose of this is to provide content for a response that has none. ICWS 
     * sends a status code of 200 and a Content-Type header of JSON even when there 
     * is no content. This causes jQuery to encounter an error (Unexpected end of 
     * input) that prevents it from calling any of the success, error, or complete 
     * callbacks. Setting the content to an empty JSON document prevents this error.
     */
    if (data == undefined || data == '')
        return '{}';
    else
        return data;
}



/****** SESSION MANAGEMENT ******
 * These functions manipulate
 */

// Loads the credentials cookie and populates fields
function loadCredsCookie() {
    var credsCookie = $.cookie(inin_credsCookie);
    if (credsCookie != undefined){
        // Convert to JSON object
        var credsCookieData = eval('('+credsCookie+')');
        console.debug('Got credentials cookie');

        // Set fields
        $('#inin-server').val(credsCookieData.server);
        $('#inin-port').val(credsCookieData.port);
        $('#inin-username').val(credsCookieData.username);
    } else {
        console.debug('no creds cookie');
    }
}

// Loads the session cookie and initializes the session variables
function loadSessionCookie(noSessionCallback, startPolling) {
    // Store info
    inin_noSessionCallback = noSessionCallback;
    inin_pollingEnabled = startPolling;

    // Check cookie
    var sessionCookie = $.cookie(inin_sessionCookie);
    if (sessionCookie != undefined) {
        // Convert to JSON object
        var sessionCookieData = eval('('+sessionCookie+')');
        console.debug('Got session cookie');

        inin_sessionId = sessionCookieData.sessionId;
        inin_csrfToken = sessionCookieData.csrfToken;
        inin_server = sessionCookieData.server;

        sendRequest('GET', inin_sessionId + '/connection', null, onCheckConnectionSuccess, onCheckConnectionError);
    } else {
        console.debug('no session cookie');

        // Notify
        if (inin_noSessionCallback) inin_noSessionCallback();
    }
}

// Success handler for GET /{sessionId}/connection
function onCheckConnectionSuccess(data, textStatus, jqXHR) {
    var jsonData = JSON.parse(data);

    if (jsonData.connectionState == 1) {
        console.debug('Connection is up');
        initialize();
    } else {
        console.debug('Connection is unavailable');
        // Cleanup
        $.removeCookie(inin_sessionCookie);
        inin_sessionId = null;
        inin_csrfToken = null;
        inin_server = null;

        // Notify
        if (inin_noSessionCallback) inin_noSessionCallback();
    }
}

// Error handler for GET /{sessionId}/connection
function onCheckConnectionError(jqXHR, textStatus, errorThrown) {
    var jsonResponse = JSON.parse(jqXHR.responseText);
    console.error(data);

    // Cleanup
    $.removeCookie(inin_sessionCookie);
    inin_sessionId = null;
    inin_csrfToken = null;
    inin_server = null;

    // Notify
    if (inin_noSessionCallback) inin_noSessionCallback();
}

// Creates a connection to ICWS
function login() {
    // Validate
    if ($('#inin-server').val().trim() == '') {
        setError('Server cannot be blank!');
        return;
    }
    if ($('#inin-port').val().trim() != '' && !isNumeric($('#inin-port').val().trim())) {
        setError('Port must be a number!');
        return;
    }
    if ($('#inin-username').val().trim() == '') {
        setError('Username cannot be blank!');
        return;
    }
    if ($('#inin-password').val() == '') {
        setError('Password cannot be blank!');
        return;
    }

    // Clear error message
    setError();

    // Set local vars 
    inin_server = $('#inin-server').val();
    if (inin_server.endsWith('/')) {
        // Remove trailing slash
        inin_server = inin_server.substring(0, inin_server.length - 1);
    }
    if ($('#inin-port').val().trim() == '') {
        // Add default ports if none specified
        if (inin_server.startsWith('https'))
            inin_server += ':8019';
        else
            inin_server += ':8018';
    } else {
        // Add specified port
        inin_server += ':' + $('#inin-port').val().trim();
    }
    inin_username = $('#inin-username').val().trim();
    inin_password = $('#inin-password').val();


    // Save credentials cookie
    var cookieData = '{ '+
        '"server":"' + $('#inin-server').val().trim() + '", ' +
        '"port":"' + $('#inin-port').val().trim() + '", ' +
        '"username":"' + $('#inin-username').val().trim() + '"' +
        ' }';
    $.cookie(inin_credsCookie, cookieData, { expires: 31 });

    // Build auth request
    loginData = {
        "__type":"urn:inin.com:connection:icAuthConnectionRequestSettings",
        "applicationName":inin_appname,
        "userID":inin_username,
        "password":inin_password                      
    };
    
    // Log in
    sendRequest("POST","connection", loginData, onLoginSuccess, onLoginError);                
}

// Success handler for POST /connection
function onLoginSuccess(data, textStatus, jqXHR) {
    var jsonData = JSON.parse(data);

    // Set session cookie
    var cookieData = '{ '+
        '"sessionId":"' + jsonData.sessionId + '", ' +
        '"csrfToken":"' + jsonData.csrfToken + '", ' +
        '"server":"' + inin_server + '"' +
        ' }';
    var expiryDate = new Date();
    expiryDate.setMinutes(expiryDate.getMinutes() + inin_sessionCookieTimeout);
    $.cookie(inin_sessionCookie, cookieData, { expires: expiryDate });

    inin_sessionId = jsonData.sessionId;
    inin_csrfToken = jsonData.csrfToken;

    initialize();
}

// Error handler for POST /connection
function onLoginError(jqXHR, textStatus, errorThrown) {
    var jsonResponse = JSON.parse(jqXHR.responseText);
    console.error(data);

    setError(jsonResponse.message);
    logout();
}

// Initializes after verifying a valid session exists
function initialize() {
    // Redirect
    if (inin_loginRedirect) {
        console.debug('Redirecting to ' + inin_loginRedirect);
        location.href = inin_loginRedirect;
        return;
    }

    // Start a polling method to get messages from the server. 
    // This is being used as a keepalive as there are no subscriptions
    if (inin_messagePollTimer || inin_pollingEnabled == false) return;
    inin_messagePollTimer = setInterval(function() {
        sendRequest("GET", inin_sessionId + "/messaging/messages", null, onCheckMessagesSuccess);
    }, inin_messagePollInterval);
}

// Success handler for POST /{sessionId}/messaging/messages
function onCheckMessagesSuccess(data, textStatus, jqXHR){
    var jsonData = JSON.parse(data);

    if(!jsonData || jsonData.length == 0){
        //console.log('No messages');
        return;
    }
    
    console.group('Processing ' + jsonData.length + ' messages');
    for(var i=0; i<jsonData.length; i++)
    {
        //console.debug(jsonData[i]);
    }
    console.groupEnd();
}

// Disconnects the session
function logout() {
    sendRequest("DELETE", inin_sessionId + "/connection", null, onLogoutSuccess, onLogoutError);     
}

// Success handler for DELETE /{sessionId}/connection
function onLogoutSuccess(data, textStatus, jqXHR) {
    var jsonData = JSON.parse(data);

    clearInterval(inin_messagePollTimer);
    $.removeCookie(inin_sessionCookie);
}

// Error handler for DELETE /{sessionId}/connection
function onLogoutError(jqXHR, textStatus, errorThrown) {
    var jsonResponse = JSON.parse(jqXHR.responseText);
    console.error(data);

    setError(data.message);
}



/****** INTERACTIONS ******
 * Functions for interactions
 */

// Creates a new generic interaction
function createInteraction() {
    // Validate
    if ($('#inin-subject').val().trim() == '') {
        setError('Subject cannot be empty!');
        return;
    }
    if ($('textarea#inin-notes').val().trim() == '') {
        setError('Notes cannot be empty!');
        return;
    }
    setError();

    /****** KNOWN ISSUE ******
     * ICWS has a bug that prevents the attributes in additionalAttributes from being set 
     * on the interaction. Because of this, it is necessary to create the interaction and 
     * then set attributes on it after it has been created. It is also known that this 
     * creates a race condition since the ACD handlers can fire before these attributes 
     * get set. There's no way arround this until the ICWS additionalAttributes bug is fixed.
     *
     * TLDR; There's an ICWS bug with additionalAttributes and a race condition due to 
     * setting attributes after the interaction has been created.
     */
    var createInteractionData = {
        "__type":"urn:inin.com:interactions:createGenericInteractionParameters",
        "queueId":{
            "queueType":2,
            "queueName":$('#inin-workgroup').val()
        },
        "initialState":5,
        "interactionDirection":1,
        "localPartyType":1,
        "remotePartyType":2,
        "remoteName":"[Hootsuite]" + 
        "[" + $('#inin-priority option:selected').text() + "] " + 
        $('#inin-subject').val()
    };

    // Create the interaction
    sendRequest('POST', inin_sessionId + '/interactions', createInteractionData, 
        onCreateInteractionSuccess, onCreateInteractionError);
}

// Success handler for POST /{sessionId}/interactions
function onCreateInteractionSuccess(data, textStatus, jqXHR) {
    var jsonData = JSON.parse(data);

    console.debug('Created interaction: ' + jsonData.interactionId);

    var setAttributeData = {
        "attributes":{
            "Hootsuite_RawData":inin_rawData,
            "Hootsuite_Subject":$('#inin-subject').val(),
            "Hootsuite_PriorityKey":$('#inin-priority').val(),
            "Hootsuite_Priority":$('#inin-priority option:selected').text(),
            "Hootsuite_ReasonKey":$('#inin-reason').val(),
            "Hootsuite_Reason":$('#inin-reason option:selected').text(),
            "Hootsuite_WorkgroupKey":$('#inin-workgroup').val(),
            "Hootsuite_Workgroup":$('#inin-workgroup option:selected').text(),
            "Hootsuite_Notes":$('textarea#inin-notes').val()
        }
    };

    // Set the attributes
    sendRequest('POST', inin_sessionId + '/interactions/' + jsonData.interactionId, setAttributeData, 
        onSetAttributeSuccess, onSetAttributeError);
}

// Error handler for POST /{sessionId}/interactions
function onCreateInteractionError(jqXHR, textStatus, errorThrown) {
    var jsonResponse = JSON.parse(jqXHR.responseText);
    console.error(data);

    alert('Error creating interaction: ' + jsonResponse.message);
}

// Success handler for POST /{sessionId}/interactions/{interactionId}
function onSetAttributeSuccess(data, textStatus, jqXHR) {
    console.debug('attributes set');

    // Hide the create interaction content
    $('#inin-maindiv').fadeOut("500", onMainFadeOutComplete);
}

// Error handler for POST /{sessionId}/interactions/{interactionId}
function onSetAttributeError(jqXHR, textStatus, errorThrown) {
    var jsonResponse = JSON.parse(jqXHR.responseText);
    console.error(data);

    alert('Error setting attributes: ' + jsonResponse.message);
}

// Handler after main div has completed hiding (after interaction is created)
function onMainFadeOutComplete() {
    // Set success message and show
    $('#inin-maindiv').html('<div class="inin-success">Interaction Created!</div><div class="inin-success2"><a href="#" onclick="closeCustomPopup()">Click to close</a></div>');
    $('#inin-maindiv').fadeIn();

    inin_closeCustomPopupTimer = setTimeout(closeCustomPopup, 3000);
}

// Closes the popup window
function closeCustomPopup() {
    // Clear the close timer if it's set
    if (inin_closeCustomPopupTimer) {
        clearTimeout(inin_closeCustomPopupTimer);
        inin_closeCustomPopupTimer = null;
    }

    // Close the popup
    hsp.closeCustomPopup("65ipi5hnjbgo8sso4gckwsw8c3ickmg3f13", getParameterByName("pid"));
}