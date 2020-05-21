'use strict';

const content_input = document.getElementById('Content');

var des_info, user_id;

function KeyDown(e) {
    console.log(e);
    if (e.key === 'Enter') Send();
}

function Send() {
    let formData = new FormData();

    formData.append("content", content_input.value);


    let request;

    request = new XMLHttpRequest();
    request.open("POST", "./api/input", true);

    request.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {
            let d = new Date();
            document.getElementById("Result").innerText = `Send @ ${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}`;

            content_input.value = "";
            content_input.focus();
        }
    };
    request.send(formData);
}


function TestSend() {
    let formData = new FormData();

    formData.append("content", encryptDES("Hello World", randomDES()));


    let request;

    request = new XMLHttpRequest();
    request.open("POST", "./api/test", true);
    request.send(formData);

}

function random16Hex() { return (0x10000 | Math.random() * 0x10000).toString(16).substr(1); }

function random64Hex() { return random16Hex() + random16Hex() + random16Hex() + random16Hex() + ""; }

function random128Hex() { return random64Hex() + random64Hex(); }

function randomDES() {
    return { key: CryptoJS.enc.Hex.parse(random128Hex()), iv: CryptoJS.enc.Hex.parse(random64Hex()) };
}

function stringToBlob(str) {
    // the first byte ignored, add 00 for padding
    var str = "00" + str;
    var hexStr = str.slice(2);
    var buf = new ArrayBuffer(hexStr.length / 2);
    var byteBuf = new Uint8Array(buf);
    for (let i = 0; i < hexStr.length; i += 2) {
        byteBuf[i / 2] = parseInt(hexStr.slice(i, i + 2), 16);
    }
    var blob = new Blob([byteBuf], { type: "application/octet-stream" });

    return blob;
}


function encryptRSA(plain) {
    var encrypt = new JSEncrypt();
    encrypt.setPublicKey(PUBLICKEY);
    var encrypted = encrypt.getKey().encrypt(plain); // return hex

    return stringToBlob(encrypted);
}


function encryptDES(plain, desInfo) { // plain: crypto-word
    var encrypted = CryptoJS.TripleDES.encrypt(plain, desInfo.key, {
        iv: desInfo.iv,
        mode: CryptoJS.mode.CBC, //ECB
        padding: CryptoJS.pad.Pkcs7
    });

    console.log('加密：', encrypted.toString());

    return encrypted;

    // return stringToBlob(encrypted.toString()); // return blob
}

function decryptDES(encrypted, desInfo) {
    var dencrypted = CryptoJS.TripleDES.decrypt(encrypted, desInfo.key, {
        iv: desInfo.iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
    console.log('解密：', dencrypted.toString(CryptoJS.enc.Utf8));

    return dencrypted;

    // const decryptedString = dencrypted.toString(CryptoJS.enc.Latin1);
    // const decryptedArray8 = Uint8Array.from(Buffer.from(decryptedString, 'latin1'));

    // return decryptedArray8;
}

(function() {
    content_input.addEventListener("keydown", KeyDown);

    des_info = randomDES();
    user_id = random64Hex();
    const registerMessage = encryptRSA(`${user_id};${des_info.key};${des_info.iv}`);

    let formData = new FormData();

    formData.append("content", registerMessage);


    let request;

    request = new XMLHttpRequest();
    request.open("POST", "./api/register", false); // sync

    request.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {
            var expected = encryptDES("hello " + user_id, des_info).toString();
            if (expected != this.responseText) alert("Oh no!!!");
        }
    };
    request.send(formData);


    // var key = "0123456789012345";
    // var s = "Hello World";

    // var base64 = CryptoJS.enc.Utf8.parse(key)
    // console.log('base64:', base64)
    // var encrypted = CryptoJS.TripleDES.encrypt(s, base64, {
    //     iv: CryptoJS.enc.Utf8.parse('00000000'),
    //     mode: CryptoJS.mode.CBC, //ECB
    //     padding: CryptoJS.pad.Pkcs7
    // });
    // console.log('加密：', encrypted.toString());

    // var Dencrypted = CryptoJS.TripleDES.decrypt(encrypted, base64, {
    //     iv: CryptoJS.enc.Utf8.parse('00000000'),
    //     mode: CryptoJS.mode.CBC,
    //     padding: CryptoJS.pad.Pkcs7
    // });
    // console.log('解密：', Dencrypted.toString(CryptoJS.enc.Utf8));

})();