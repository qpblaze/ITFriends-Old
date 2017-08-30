import requests

RUN_URL = u'http://api.hackerearth.com/code/run/'
CLIENT_SECRET = 'b3c059c923cb6fe7ed4f51a8932c1fe6ec600a15'

data = {
    'client_secret': CLIENT_SECRET,
    'async': 0,
    'source': code,
    'lang': language,
    'time_limit': 5,
    'memory_limit': 262144,
    'input': input
}

r = requests.post(RUN_URL, data=data)
result = r.json()