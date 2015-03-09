var express = require('express');
var app = express();

app.set('port', (process.env.PORT || 5000));
app.use(express.static(__dirname + '/public'));

app.get('/', function(request, response) {
	var currentdate = new Date(); 
	var datetime = currentdate.getDate() + "/"
					+ (currentdate.getMonth()+1)  + "/" 
					+ currentdate.getFullYear() + " @ "  
					+ currentdate.getHours() + ":"  
					+ currentdate.getMinutes() + ":" 
					+ currentdate.getSeconds();
  response.send(datetime);
});

app.listen(app.get('port'), function() {
  console.log("Node app is running at localhost:" + app.get('port'));
});
