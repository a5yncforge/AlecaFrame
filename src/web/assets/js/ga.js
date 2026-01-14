
class MeasurementProtocol {
    constructor(measurement_id, api_secret, client_id, debug) {
        this._measurement_id = measurement_id ? measurement_id : id;
        this._api_secret = api_secret ? api_secret : secret;
        this._client_id = client_id;
        // This may take a little. If you send events before it is ready, they will not have a user_id defined!
        overwolf.profile.getCurrentUser((result) => {
            if (result.success) {
                this._user_id = result.machineId;
            } else {
                this._user_id = "no-client"
            }
        })
        this._debug = debug ? debug : false;
    }

    pageview(pageURL, title) {
        this.fireEvent('page_view', {
            page_location: pageURL,
            page_title: title,
        })
    }

    fireEvent(hit, params) {
        fetch(`https://www.google-analytics.com/${this._debug ? "debug/" : ""}mp/collect?measurement_id=${this._measurement_id}&api_secret=${this._api_secret}`, {
            method: "POST",
            body: JSON.stringify({
                client_id: this._client_id,
                user_id: this._user_id,
                events: [{
                    // Event names must start with an alphabetic character.
                    name: hit,
                    params:
                    {
                        ...params,
                        /* session_id - Assuming a valid gtag session (keep in mind that session_start is a gtag reserved event) */
                    }
                }]
                /* timestamp_micros - You can set this if you want, but it defaults to now. You CAN use it to backdate events.
                 * For more details, see https://developers.google.com/analytics/devguides/collection/protocol/ga4/reference?client_type=gtag#reserved_user_property_names
                 */
            })
        }).then((response) => {
            if (this._debug) console.log(response);
        });
    }
}

var newGAID = localStorage.getItem["ga4id"];
if (!newGAID) {
    newGAID = parseInt(Math.random() * 1000000000) + "-" + parseInt(Math.random() * 1000000000);
    localStorage["ga4id"] = newGAID;
}

var newGA = new MeasurementProtocol("G-TFLFSSG5PH", "NGMPin0jSdSScbFUZwW8UQ", newGAID, false);







// Standard Google Universal Analytics code
(function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function()
   {
      (i[r].q=i[r].q||[]).push(arguments)
   },
   i[r].l=1*new Date();a=s.createElement(o),
   m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
   // Note: https protocol here
})(window,document,'script','https://www.google-analytics.com/analytics.js','ga');

//Uncomment to disable analytics
//ga('set', 'sendHitTask', null);


ga('create', 'UA-67909748-3', 'auto');

// Removes failing protocol check. @see: http://stackoverflow.com/a/22152353/1958200
ga('set', 'checkProtocolTask', function(){}); 
ga('require', 'displayfeatures');

ga('send', 'pageview');



function setUserID()
{
	if(typeof overwolf == 'undefined') return;
	if(overwolf.windows.getMainWindow().plugin==undefined || overwolf.windows.getMainWindow().plugin.get()==undefined)
	{
		setTimeout(setUserID,1500);
		return;
	}
	overwolf.windows.getMainWindow().plugin.get().getAnalyticsName((success,name)=>{
		if (!success)
		{
			setTimeout(setUserID,1500);
		}else
		{			
			//ga('set', 'userId', name);			
		}
	});
}

function sendPage(pageName)
{
    ga("send", "pageview", pageName);	
    newGA.pageview(pageName, pageName);
}


setUserID();

console.log("GA setup complete")