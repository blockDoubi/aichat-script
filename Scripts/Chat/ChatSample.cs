using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebGLSupport;
using System.IO;
using static LLM;
using UnityEditor;
using static ChatSample;
using System;
using TMPro;
using UnityEngine.TextCore.Text;

public class ChatSample : MonoBehaviour
{
    /// <summary>
    /// 聊天配置
    /// </summary>
    [SerializeField] private ChatSetting m_ChatSettings;
    #region ui
    /// <summary>
    /// 聊天UI窗
    /// </summary>
    [SerializeField] private GameObject m_ChatPanel;
    /// <summary>
    /// 输入的信息
    /// </summary>
    [SerializeField] public InputField m_InputWord;
    /// <summary>
    /// 返回的信息
    /// </summary>
    [SerializeField] private Text m_TextBack;
    /// <summary>
    /// 播放声音
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;
    


    #endregion

    #region 示例人物动画
    /// <summary>
    /// 动画控制器
    /// </summary>
    [SerializeField] private Animator m_Animator;

    #endregion
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            SendData();
        }
    }
    private void Awake()
    {
        //m_CommitMsgBtn.onClick.AddListener(delegate { SendData(); });
        RegistButtonEvent();
        InputSettingWhenWebgl();
        SettingInitialize();
    }

    #region 消息发送

    /// <summary>
    /// webgl时处理，支持中文输入
    /// </summary>
    private void InputSettingWhenWebgl()
    {
#if UNITY_WEBGL
        m_InputWord.gameObject.AddComponent<WebGLSupport.WebGLInput>();
#endif
    }


    /// <summary>
    /// 发送信息
    /// </summary>
    public void SendData()
    {
        if (m_InputWord.text.Equals(""))
            return;

        //添加记录聊天
        m_ChatHistory.Add(m_InputWord.text);
        //提示词
        string _msg = m_InputWord.text;

        //发送数据
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "正在思考中...";

        //切换思考动作
        SetAnimator("state", 1);
    }
    /// <summary>
    /// 带文字发送
    /// </summary>
    /// <param name="_postWord"></param>
    public void SendData(string _postWord)
    {
        if (_postWord.Equals(""))
            return;

        //添加记录聊天
        m_ChatHistory.Add(_postWord);
        //提示词
        string _msg = _postWord;

        //发送数据
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "正在思考中...";

        //切换思考动作
        SetAnimator("state", 1);
    }

    /// <summary>
    /// AI回复的信息的回调
    /// </summary>
    /// <param name="_response"></param>
    private void CallBack(string _response)
    {
        _response = _response.Trim();
        m_TextBack.text = "";


        //记录聊天
        m_ChatHistory.Add(_response);

        if (m_ChatSettings.m_TextToSpeech == null)
            return;

        m_ChatSettings.m_TextToSpeech.Speak(_response, PlayVoice);
    }

#endregion

#region 语音输入
    /// <summary>
    /// 语音识别返回的文本是否直接发送至LLM
    /// </summary>
    [SerializeField] private bool m_AutoSend = true;
    /// <summary>
    /// 语音输入的按钮
    /// </summary>
    [SerializeField] private Button m_VoiceInputBotton;
    /// <summary>
    /// 录音按钮的文本
    /// </summary>
    [SerializeField]private Text m_VoiceBottonText;
    /// <summary>
    /// 录音的提示信息
    /// </summary>
    [SerializeField] private Text m_RecordTips;
    /// <summary>
    /// 语音输入处理类
    /// </summary>
    [SerializeField] private VoiceInputs m_VoiceInputs;
    /// <summary>
    /// 注册按钮事件
    /// </summary>
    private void RegistButtonEvent()
    {
        if (m_VoiceInputBotton == null || m_VoiceInputBotton.GetComponent<EventTrigger>())
            return;

        EventTrigger _trigger = m_VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

        //添加按钮按下的事件
        EventTrigger.Entry _pointDown_entry = new EventTrigger.Entry();
        _pointDown_entry.eventID = EventTriggerType.PointerDown;
        _pointDown_entry.callback = new EventTrigger.TriggerEvent();

        //添加按钮松开事件
        EventTrigger.Entry _pointUp_entry = new EventTrigger.Entry();
        _pointUp_entry.eventID = EventTriggerType.PointerUp;
        _pointUp_entry.callback = new EventTrigger.TriggerEvent();

        //添加委托事件
        _pointDown_entry.callback.AddListener(delegate { StartRecord(); });
        _pointUp_entry.callback.AddListener(delegate { StopRecord(); });

        _trigger.triggers.Add(_pointDown_entry);
        _trigger.triggers.Add(_pointUp_entry);
    }

    /// <summary>
    /// 开始录制
    /// </summary>
    public void StartRecord()
    {
        m_VoiceBottonText.text = "正在录音中..."; 
        m_VoiceInputs.StartRecordAudio();
    }
    /// <summary>
    /// 结束录制
    /// </summary>
    public void StopRecord()
    {
        m_VoiceBottonText.text = "按住按钮，开始录音"; 
        m_RecordTips.text = "录音结束，正在识别...";
        m_VoiceInputs.StopRecordAudio(AcceptClip);
    }

    /// <summary>
    /// 处理录制的音频数据
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptData(byte[] _data)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_data, DealingTextCallback);
    }

    /// <summary>
    /// 处理录制的音频数据
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptClip(AudioClip _audioClip)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_audioClip, DealingTextCallback);
    }
    /// <summary>
    /// 处理识别到的文本
    /// </summary>
    /// <param name="_msg"></param>
    private void DealingTextCallback(string _msg)
    {
        m_RecordTips.text = _msg;
        StartCoroutine(SetTextVisible(m_RecordTips));
        //自动发送
        if (m_AutoSend)
        {
            SendData(_msg);
            return;
        }

        m_InputWord.text = _msg;
    }

    private IEnumerator SetTextVisible(Text _textbox)
    {
        yield return new WaitForSeconds(3f);
        _textbox.text = "";
    }

#endregion

#region 语音合成

    private void PlayVoice(AudioClip _clip, string _response)
    {
        if(_response == "鉴权错误！请检查是否正确填写token。token的格式应为sk- xxxxxxxxxxxxxx")
        {
            //Debug.Log(_response);
            m_WriteState = true;
            StartCoroutine(SetTextPerWord(_response));
        }
        else
        {
            m_AudioSource.clip = _clip;
            m_AudioSource.Play();
            Debug.Log("音频时长：" + _clip.length);
            //开始逐个显示返回的文本
            m_WriteState = true;
            StartCoroutine(SetTextPerWord(_response));

           
        }
        //切换到说话动作
        SetAnimator("state", 2);
    }

#endregion

#region 文字逐个显示
    //逐字显示的时间间隔
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //是否显示完成
    [SerializeField] private bool m_WriteState = false;
    private IEnumerator SetTextPerWord(string _msg)
    {
        Debug.Log(_msg);
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //更新显示的内容
            m_TextBack.text = _msg.Substring(0, currentPos);

            m_WriteState = currentPos < _msg.Length;

        }

        //切换到等待动作
        SetAnimator("state",0);
    }

    #endregion
    private void SetAnimator(string _para, int _value)
    {
        if (m_Animator == null)
            return;

        m_Animator.SetInteger(_para, _value);
    }

    #region 聊天记录
    //保存聊天记录
    [SerializeField] private List<string> m_ChatHistory;
    //缓存已创建的聊天气泡
    [SerializeField] private List<GameObject> m_TempChatBox;
    //聊天记录显示层
    [SerializeField] private GameObject m_HistoryPanel;
    //聊天文本放置的层
    [SerializeField] private RectTransform m_rootTrans;
    //发送聊天气泡
    [SerializeField] private ChatPrefab m_PostChatPrefab;
    //回复的聊天气泡
    [SerializeField] private ChatPrefab m_RobotChatPrefab;
    //滚动条
    [SerializeField] private ScrollRect m_ScroTectObject;
    //获取聊天记录
    public void OpenAndGetHistory()
    {
        m_ChatPanel.SetActive(false);
        m_HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(GetHistoryChatInfo());
    }
    //返回
    public void BackChatMode()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
        m_SettingPanel.SetActive(false);
    }

    //清空已创建的对话框
    private void ClearChatBox()
    {
        while (m_TempChatBox.Count != 0)
        {
            if (m_TempChatBox[0])
            {
                Destroy(m_TempChatBox[0].gameObject);
                m_TempChatBox.RemoveAt(0);
            }
        }
        m_TempChatBox.Clear();
    }

    //获取聊天记录列表
    private IEnumerator GetHistoryChatInfo()
    {

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < m_ChatHistory.Count; i++)
        {
            if (i % 2 == 0)
            {
                ChatPrefab _sendChat = Instantiate(m_PostChatPrefab, m_rootTrans.transform);
                _sendChat.SetText(m_ChatHistory[i]);
                m_TempChatBox.Add(_sendChat.gameObject);
                continue;
            }

            ChatPrefab _reChat = Instantiate(m_RobotChatPrefab, m_rootTrans.transform);
            _reChat.SetText(m_ChatHistory[i]);
            m_TempChatBox.Add(_reChat.gameObject);
        }

        //重新计算容器尺寸
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
        StartCoroutine(TurnToLastLine());
    }

    private IEnumerator TurnToLastLine()
    {
        yield return new WaitForEndOfFrame();
        //滚动到最近的消息
        m_ScroTectObject.verticalNormalizedPosition = 0;
    }


    #endregion




    #region//设置界面相关
    //设置显示层
    [SerializeField] private GameObject m_SettingPanel;
    [SerializeField] private AudioSource m_BGMSource;
    [SerializeField] private GameObject m_normalModel;
    [SerializeField] private GameObject m_ARModel;
    [SerializeField] private GameObject m_normalGroup;
    [SerializeField] private GameObject m_ARGroup;
    //设置文件json存储地址
    private string settingURL;
    //UI组件
    public Slider voiceVolume_UI;
    public CustomDropdown character_UI;
    public Slider BGMVolume_UI;
    public SwitchManager AR_UI;
    public CustomInputField token_UI;
    public CustomDropdown model_UI;

    [System.Serializable]
    public class SettingData
    {
        public float voiceVolume;
        public OpenAITextToSpeech.VoiceType character;
        public float BGMVolume;
        public bool isARMode;
        public string token;
        public string LLM_Model;
        public SettingData(float voiceVolume=50, OpenAITextToSpeech.VoiceType character=OpenAITextToSpeech.VoiceType.nova, float bGMVolume=50, bool isARMode=false, string token="", string lLM_Model="gpt-3")
        {
            this.voiceVolume = voiceVolume;
            this.character = character;
            BGMVolume = bGMVolume;
            this.isARMode = isARMode;
            this.token = token;
            LLM_Model = lLM_Model;
        }
    }
    public SettingData m_settingData=new SettingData();
    //设置初始化
    public void SettingInitialize()
    {
        settingURL = Application.persistentDataPath + "\\SettingData.json";
        Debug.Log(settingURL);
        string js = JsonUtility.ToJson(m_settingData);
        if (System.IO.File.Exists(settingURL))
        {
            //将json文件写入m_settingData
            using (StreamReader sr = File.OpenText(settingURL))
            {
                string text=sr.ReadToEnd();
                m_settingData = JsonUtility.FromJson<SettingData>(text);
                sr.Close();
            }
            //应用到客户端设置
            ChangeCharacterVolume(m_settingData.voiceVolume);
            ChangeUIPerform_Voice(m_settingData.voiceVolume);

            ChangeBGMVolume(m_settingData.BGMVolume);
            ChangeUIPerform_BGM(m_settingData.BGMVolume);

            SetCharacter(m_settingData.character);

            //if (m_settingData.isARMode)
            //{
            //    TransferToAR();
            //}
            //else
            //{
            //    TransfertToNormal();
            //}


            InputToken(m_settingData.token);
            ChangeUIPerform_Token(m_settingData.token);

            SetModel(m_settingData.LLM_Model);
        }
        else
        {
            //将m_settingData写入json文件
            using (StreamWriter sw = new StreamWriter(settingURL))
            {
                //保存数据
                sw.WriteLine(js);
                //关闭文档
                sw.Close();
                sw.Dispose();
            }
        }


    }
    //更新设置文件
    public void UpdateSettingData()
    {
        string js = JsonUtility.ToJson(m_settingData);
        using (StreamWriter sw = new StreamWriter(settingURL))
        {
            //保存数据
            sw.WriteLine(js);
            //关闭文档
            sw.Close();
            sw.Dispose();
        }
    }
    //打开设置页面
    public void OpenSettingPanel()
    {
        m_ChatPanel.SetActive(false);
        m_SettingPanel.SetActive(true);
    }
    //保存并关闭设置页面
    public void FinishSetting()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
        m_SettingPanel.SetActive(false);
        UpdateSettingData();
    }


    //调节说话音量
    public void ChangeCharacterVolume(float value)
    {
        m_AudioSource.volume = value/100;
        m_settingData.voiceVolume=value;
    }
    public void ChangeUIPerform_Voice(float value)
    {
        voiceVolume_UI.value = value;
    }
    //调节BGM音量
    public void ChangeBGMVolume(float value)
    {
        m_BGMSource.volume = value/100;
        m_settingData.BGMVolume = value;
    }
    public void ChangeUIPerform_BGM(float value)
    {
        BGMVolume_UI.value = value;
    }
    //选择说话角色
    public void SetCharacter(int value)//传整型方法（用于UI）
    {
        OpenAITextToSpeech.VoiceType characterName= OpenAITextToSpeech.VoiceType.nova;
        switch (value)
        {
            case 0:
                characterName = OpenAITextToSpeech.VoiceType.alloy;
                break;
            case 1:
                characterName = OpenAITextToSpeech.VoiceType.echo;
                break;
            case 2:
                characterName = OpenAITextToSpeech.VoiceType.fable;
                break;
            case 3:
                characterName = OpenAITextToSpeech.VoiceType.onyx;
                break;
            case 4:
                characterName = OpenAITextToSpeech.VoiceType.nova;
                break;
            case 5:
                characterName = OpenAITextToSpeech.VoiceType.shimmer;
                break;
        }
        TTS tts = m_ChatSettings.m_TextToSpeech;
        if(tts is OpenAITextToSpeech)
        {
            (tts as OpenAITextToSpeech).m_Voice = characterName;
            m_settingData.character = characterName;
        }
    }
    public void SetCharacter(OpenAITextToSpeech.VoiceType character)//传枚举型方法（用于初始化）
    {
        TTS tts = m_ChatSettings.m_TextToSpeech;
        if (tts is OpenAITextToSpeech)
        {
            (tts as OpenAITextToSpeech).m_Voice = character;
            m_settingData.character = character;
        }
        character_UI.SetDropdownIndex((int)character);
    }
    //切换至AR模式
    public void TransferToAR()
    {
        m_normalGroup.SetActive(false);
        m_ARGroup.SetActive(true);      
        m_Animator= m_ARModel.GetComponent<Animator>();
        m_settingData.isARMode = true;
    }
    //切换至普通模式
    public void TransfertToNormal()
    {
        m_ARGroup.SetActive(false);
        m_normalGroup.SetActive(true);
        m_Animator = m_normalModel.GetComponent<Animator>();
        m_settingData.isARMode = false;
    }
    //输入token
    public void InputToken(string token)
    {
        m_settingData.token = token;    
        if (m_ChatSettings.m_ChatModel is chatgptTurbo)
        {
            (m_ChatSettings.m_ChatModel as chatgptTurbo).api_key = token;
        }
        else
        {
            Debug.LogWarning("请设置LLM为chatgptTurbo！");
        }
        if(m_ChatSettings.m_TextToSpeech is OpenAITextToSpeech)
        {
            (m_ChatSettings.m_TextToSpeech as OpenAITextToSpeech).api_key=token;
        }
    }
    public void ChangeUIPerform_Token(string token)
    {
        token_UI.inputText.text = token;
        StartCoroutine(setUIAnime_token());
    }
    //选择大语言模型
    public void SetModel(int index)
    {
        if(m_ChatSettings.m_ChatModel is chatgptTurbo)
        {
            chatgptTurbo gpt = m_ChatSettings.m_ChatModel as chatgptTurbo;
            string modelSelector="";
            switch (index)
            {
                case 0:
                    modelSelector = "gpt-3";
                    break;
                case 1:
                    modelSelector = "spark";
                    break;
                case 2:
                    modelSelector = "baidu";
                    break;
                case 3:
                    modelSelector = "glm";
                    break;
                case 4:
                    modelSelector = "gpt-4";
                    break;
                case 5:
                    modelSelector = "ali";
                    break;
            }
            gpt.m_gptModel=modelSelector;
            m_settingData.LLM_Model = modelSelector;
        }
        else
        {
            Debug.LogWarning("请设置LLM为chatgptTurbo！");
        }
    }
    public void SetModel(string modelSelector)
    {
        if (m_ChatSettings.m_ChatModel is chatgptTurbo)
        {
            chatgptTurbo gpt = m_ChatSettings.m_ChatModel as chatgptTurbo;
            gpt.m_gptModel = modelSelector;
            m_settingData.LLM_Model = modelSelector;
        }
        else
        {
            Debug.LogWarning("请设置LLM为chatgptTurbo！");
        }
        Dictionary<string, int> map_character = new Dictionary<string, int>()
        {
            {"gpt-3",0 },
            {"spark",1 },
            {"baidu",2 },
            {"glm",3 },
            {"gpt-4",4 },
            {"ali",5 },
        };
        model_UI.SetDropdownIndex(map_character[modelSelector]);
    }



    //延时动画（防止被UI初始化覆盖）
    private IEnumerator setUIAnime_token()
    {
        yield return new WaitForSeconds(0.2f);
        token_UI.UpdateStateInstant();
    }
    #endregion


}
