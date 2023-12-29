using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        // Use the Assert class to test conditions
        string key = "MjlmNzkzNmZkMDQ2OTc0ZDdmNGE2ZTZi";
        string buider = "host: spark-api.xf-yun.com\ndate: Fri, 05 May 2023 10:43:39 GMT\nGET /v1.1/chat HTTP/1.1";
        string result=HMACsha256(key, buider);
        Assert.AreEqual("z5gHdu3pxVV4ADMyk467wOWDQ9q6BQzR3nfMTjc/DaQ=", result);
    }
    public string HMACsha256(string apiSecretIsKey, string buider)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(apiSecretIsKey);
        System.Security.Cryptography.HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
        byte[] date = System.Text.Encoding.UTF8.GetBytes(buider);
        date = hMACSHA256.ComputeHash(date);
        hMACSHA256.Clear();

        return Convert.ToBase64String(date);

    }
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
