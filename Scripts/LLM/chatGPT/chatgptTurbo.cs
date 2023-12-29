using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class chatgptTurbo : LLM
{
    public chatgptTurbo()
    {
        //url = "http://api.openai.com/v1/chat/completions";
        url = "https://api.alpacabro.cc/v1/chat/completions";
    }

    /// <summary>
    /// api key
    /// </summary>
    [SerializeField] public string api_key;
    /// <summary>
    /// AI设定
    /// </summary>
    public string m_SystemSetting = string.Empty;
    /// <summary>
    /// gpt-3.5-turbo
    /// </summary>
    public string m_gptModel = "gpt-3.5-turbo";


    private void Start()
    {
        //运行时，添加AI设定
        m_DataList.Add(new SendData("system", m_SystemSetting));
    }
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <returns></returns>
    public override void PostMsg(string _msg, Action<string> _callback)
    {
        base.PostMsg(_msg, _callback);
    }

    /// <summary>
    /// 调用接口
    /// </summary>
    /// <param name="_postWord"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public override IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        stopwatch.Restart();
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));
            
            //test
            //request.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");
            //test/
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msgBack = request.downloadHandler.text;
                Debug.Log(_msgBack);
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                if (_textback != null && _textback.choices.Count > 0)
                {

                    string _backMsg = _textback.choices[0].message.content;
                    Debug.Log(_backMsg);
                    //添加记录
                    m_DataList.Add(new SendData("assistant", _backMsg));
                    _callback(_backMsg);
                }

            }else if(request.responseCode == 401)
            {
                m_DataList.Add(new SendData("assistant", "鉴权错误！请检查是否正确填写token。token的格式应为sk- xxxxxxxxxxxxxx"));
                _callback("鉴权错误！请检查是否正确填写token。token的格式应为sk- xxxxxxxxxxxxxx");
            }

            stopwatch.Stop();
            Debug.Log("chatgpt耗时："+ stopwatch.Elapsed.TotalSeconds);
        }
    }

    #region 数据包

    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
    }

    [Serializable]
    public class MessageBack
    {
        public string id;
        public string created;
        public string model;
        public List<MessageBody> choices;
    }
    [Serializable]
    public class MessageBody
    {
        public Message message;
        public string finish_reason;
        public string index;
    }
    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    #endregion

}
