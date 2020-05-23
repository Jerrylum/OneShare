'use strict';

const content_input = document.getElementById('Content');

let des_info, user_id, next_msg_Id, ws;

function KeyDown(e) {
    if (typeof window.orientation === 'undefined') { // PC
        if (e.key === 'Enter' && !e.shiftKey)
            SendString();
    }
}

function SendString() {
    content_input.focus();

    if (user_id != undefined) { // registed
        // bit: | 3 byte | 4 byte | ....
        //      |  type  | msg id | data
        var content = encryptDES('str' + next_msg_Id + content_input.value, des_info).toString();

        ws.send(content);

        DisplayMyMessage(user_id.slice(0, 6), content_input.value);
    }
}

function DisplayMyMessage(username, msg) {
    let now = new Date();

    let node = document.createElement('li');
    node.classList.add('clearfix', 'my-message');
    node.innerHTML = `<div class="message-data noselect">
                          <span class="message-data-time">${new Date().toTimeString().split(' ')[0]}</span> &nbsp; &nbsp;
                          <span class="message-data-name">${username}</span>
                      </div>
                      <div class="message">${msg}</div>`

    document.querySelector('.chat-history ul').appendChild(node);
    node.scrollIntoView();
}

function DisplayOtherMessage(username, msg) {
    let now = new Date();

    let node = document.createElement('li');
    node.classList.add('other-message');
    node.innerHTML = `<div class="message-data noselect">
                          <span class="message-data-name">${username}</span> &nbsp; &nbsp;
                          <span class="message-data-time">${new Date().toTimeString().split(' ')[0]}</span>
                      </div>
                      <div class="message">${msg}</div>`;

    document.querySelector('.chat-history ul').appendChild(node);
    node.scrollIntoView();
}

function getMsgId() { return { now: next_msg_Id, next: (next_msg_Id = random16Hex()) }; };

function random16Hex() { return (0x10000 | Math.random() * 0x10000).toString(16).substr(1); }

function random64Hex() { return random16Hex() + random16Hex() + random16Hex() + random16Hex() + ''; }

function random128Hex() { return random64Hex() + random64Hex(); }

function randomDES() {
    return { key: CryptoJS.enc.Hex.parse(random128Hex()), iv: CryptoJS.enc.Hex.parse(random64Hex()) };
}

function HexStrToBlob(str) {
    // the first byte ignored, add 00 for padding
    str = '00' + str;
    let hexStr = str.slice(2);
    let buf = new ArrayBuffer(hexStr.length / 2);
    let byteBuf = new Uint8Array(buf);
    for (let i = 0; i < hexStr.length; i += 2) {
        byteBuf[i / 2] = parseInt(hexStr.slice(i, i + 2), 16);
    }
    let blob = new Blob([byteBuf], { type: 'application/octet-stream' });

    return blob;
}


function encryptRSA(plain) {
    let encrypt = new JSEncrypt();
    encrypt.setPublicKey(PUBLICKEY);
    let encrypted = encrypt.getKey().encrypt(plain); // return hex

    return HexStrToBlob(encrypted);
}


function encryptDES(plain, desInfo) { // plain: crypto-word
    let encrypted = CryptoJS.TripleDES.encrypt(plain, desInfo.key, {
        iv: desInfo.iv,
        mode: CryptoJS.mode.CBC, //ECB
        padding: CryptoJS.pad.Pkcs7
    });

    console.log('加密：', encrypted.toString());

    return encrypted;

    // return stringToBlob(encrypted.toString()); // return blob
}

function decryptDES(encrypted, desInfo) { // encrypted: crypto-word
    let dencrypted = CryptoJS.TripleDES.decrypt({ ciphertext: encrypted }, desInfo.key, {
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
    content_input.addEventListener('keydown', KeyDown);

    ws = new WebSocket('ws://' + window.location.hostname + ':' + (window.location.port - 0 + 1) + '/default')

    ws.onopen = () => {
        console.log('open connection')


        des_info = randomDES();
        const registerMessage = encryptRSA(`reg;${des_info.key};${des_info.iv}`);

        ws.send(registerMessage);
    }

    ws.onclose = () => {
        console.log('close connection')
    }

    //接收 Server 發送的訊息
    ws.onmessage = event => {
        let encrypted = CryptoJS.enc.Hex.parse(event.data);
        let rtnRaw = decryptDES(encrypted, des_info).toString(CryptoJS.enc.Utf8);

        let splitted = rtnRaw.split(';');
        let a = splitted[0];
        let b = splitted[1];
        let c = splitted[2];

        if (user_id == undefined) {
            if (a == 'acc') {
                user_id = b
                next_msg_Id = c;
            }
        } else {
            if (a == 'res') {
                if (b == next_msg_Id) {
                    content_input.value = '';
                    next_msg_Id = c;
                } else
                    console.log('Oh nooo');
            } else if (a == 'msg') {
                DisplayOtherMessage(b.slice(0, 6), c);
            }
        }
    }

})();