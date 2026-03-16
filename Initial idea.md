the app name is going to be musicratic

so i want to build a mobile app using flutter/dart that enables users to play the music they want in a hub
per example, i have a coffe shop, i have my own coffe shop music list that plays all day long
then a user wants to play a song of his own or one from the list - so he can pay some virtual coins to play it or do a question for the current users in the coffe shop with the app to play it collectively for free - then, if the song is approved by the coffe shop list owner, is played

users can vote up or down for current playing songs in its first minute, so if song recieves atleast 65% of downvotes its skipped unless list owner skips it manually - this only can happend if this track was purposed by a non-owner user - if virtual coins were used to pay the track play, 50% of tokens are refunded

a user can attach himself only to one hub at a time - for making it easy to develop, at this time, user can only attach for 1 hour; later, when the product generates some income, will incorporate location based attachments so users are attached eternally only if they are less than X meters from the actual 'hub' and reatached when they access the app if in range

there are gonna be two types of users, list owners and visitors
list owner publish a list to a hub they manage
visitors will purpouse music traks
both will vote for purpoused tracks - list owners have priority, so if they downvote a song is skipped anytime, they dont apply for the first-minute rule

hubs are a server-like system - they publish a backend that is managed by a super list owner
super list owner can designate sub hub managers and sub list owners
then, when a list is configured, it will play the list in order or shuffled
hub will also count the upvotes and downvotes for the list tracks and will reproduce more the most positively voted songs
once a week will prompt the list owner a list of most downvoted songs for replacing them
once a month will prompt with the most user reproduced tracks with the most upvotes for incorporating them to the list with 0 total votes so statistics 'restart' but keeping the user-proposed track statistics while in the list - if its removed, from list, stadistics restart

for users to connecto to the hub, hub will have to expose a qr with a directlink and a identifier so hub can be find accross the internet

once the core 'hub for shops' is implemented and fully working, a 'portable' hub will be introduced to normal users
this hub will designate a mood - driving, home party... - this mood will tell the hub how to work
when home party mood is designated, no list owner is selected but a list composer is - it will manage the hub, will set a default list and then will vote and count as a normal user
when driving mood is designated, a driver is designated - the driver will select a list of his own, this tracks are not skippable but the driver will, driver will only downvote - rest of the users will add songs to play normally - driver can only skip by downvote 5 user-proposed songs in a row then atleast one proposed song is played -- this mood will the party to stop and change the driver every two hours -- also will work the most eficiently to save battery as much as it can -- have to be able to make a hotspot so others can connect to drivers phone or via bluetooth, nfc or internet or other strategy

three components are found:
 - hub
 - list owner frontend
 - normal user frontend

hub and list owner frontends must be web, mobile and desktop apps
normal users will be web and mobile only

for revenue, normal users will use virtual coins to play songs, each song has a different price depending on how hottest the song is (more concurrent reproductions over the total hubs more hot), also duration affects to song cost always rounding down (one minute, one coin, two and a half minutes, 2 coins...)
hubs will cost as a annual, monthly or event duration suscriptions - a free tier is given for a month, then payment is needed
moods will also cost, but they will be closed-time event costed making attached users to pay 0 coins for each song with a limitation of 5 users - adds by 2 users making cost rise by thirds - time increments by 35% to cost (1 mood for 24h 5 users -> 5€ => 1 mood for 48h 5 users -> 8€ => 1 mood for 24h for 10 users -> 8€)
also a free tier is offered with advertisements every hour or every non-list owner song after if non-hub license is paid

spotify, youtube music and other sources can be configured for trak sourcing
if hub paid license is active, also local storage could be used

since social is what gathers people, we need a way so hub offers reviewing, most voted songs and lists, user profiles, hub finding (so if i like jazz, i can find a hub that plays it), user public lists and hub owners public lists, and a discover live music spots and some kind of pushup for live music
