/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */

const functions = require("firebase-functions");
const admin = require("firebase-admin");

// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// exports.helloWorld = onRequest((request, response) => {
//   logger.info("Hello logs!", {structuredData: true});
//   response.send("Hello from Firebase!");
// });

admin.initializeApp();

exports.sendPushOnScan = functions.firestore
    .document("scans/{scanId}")
    .onCreate(async (snap, context) => {
      const scan = snap.data();
      const payload = {
        notification: {
          title: "Nieuwe scan!",
          body: `Barcode: ${scan.barcode} is gescand.`,
        },
      };
      // Stuur naar het topic 'scans'
      await admin.messaging().sendToTopic("scans", payload);
    });
