Instructions to get this project working with Photon Bolt
=======

* Install Photon Bolt from https://www.assetstore.unity3d.com/en/#!/content/41330
* Run through their setup instructions http://doc.photonengine.com/en/bolt/current/setup/installing
* Create a new Bolt Event called `TestyBoltEvent`, with one propery `RandomValue` which is an integer, limited from 0 - 100 in value

* Run two versions of the app on the same network, choosing `Server` as one, and `Client` as the other
* Wait until connection message appears
* Send event from each side, and you will see the information from it
