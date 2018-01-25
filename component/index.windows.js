'use strict';

var {
  NativeModules,
  DeviceEventEmitter,
} = require('react-native');

var RNPushNotification = NativeModules.RNPushNotification;
var _notifHandlers = new Map();

var DEVICE_NOTIF_EVENT = 'remoteNotificationReceived';
var NOTIF_REGISTER_EVENT = 'remoteNotificationsRegistered';
var REMOTE_FETCH_EVENT = 'remoteFetch';

var NotificationsComponent = function() {

};

NotificationsComponent.prototype.getInitialNotification = function () {
	/* Void */
};

NotificationsComponent.prototype.requestPermissions = function(senderID: string) {
	/* Void */
};

NotificationsComponent.prototype.cancelLocalNotifications = function(details: Object) {
	/* Void */
};

NotificationsComponent.prototype.cancelAllLocalNotifications = function() {
	/* Void */
};

NotificationsComponent.prototype.presentLocalNotification = function(details: Object) {
	/* Void */
};

NotificationsComponent.prototype.scheduleLocalNotification = function(details: Object) {
	/* Void */
};

NotificationsComponent.prototype.setApplicationIconBadgeNumber = function(number: number) {
	/* Void */
};

NotificationsComponent.prototype.abandonPermissions = function() {
	/* Void */
};

NotificationsComponent.prototype.checkPermissions = function(callback: Function) {
	/* Void */
};

NotificationsComponent.prototype.addEventListener = function(type: string, handler: Function) {
	var listener;
	if (type === 'notification') {
		listener =  DeviceEventEmitter.addListener(
			DEVICE_NOTIF_EVENT,
			function(notifData) {
				var data = JSON.parse(notifData.dataJSON);
				handler(data);
			}
		);
	} else if (type === 'register') {
		listener = DeviceEventEmitter.addListener(
			NOTIF_REGISTER_EVENT,
			function(registrationInfo) {
				handler(registrationInfo.deviceToken);
			}
		);
	} else if (type === 'remoteFetch') {
		listener = DeviceEventEmitter.addListener(
			REMOTE_FETCH_EVENT,
			function(notifData) {
				var notificationData = JSON.parse(notifData.dataJSON)
				handler(notificationData);
			}
		);
	}

	_notifHandlers.set(type, listener);
};

NotificationsComponent.prototype.removeEventListener = function(type: string, handler: Function) {
	var listener = _notifHandlers.get(type);
	if (!listener) {
		return;
	}
	listener.remove();
	_notifHandlers.delete(type);
}

NotificationsComponent.prototype.registerNotificationActions = function(details: Object) {
	/* Void */
}

NotificationsComponent.prototype.clearAllNotifications = function() {
	/* Void */
}

module.exports = {
	state: false,
	component: new NotificationsComponent()
};

