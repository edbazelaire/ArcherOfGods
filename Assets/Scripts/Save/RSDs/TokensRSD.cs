using System.Collections.Generic;
using Unity.Services.Relay.Models;

namespace Save.RSDs
{
    public class TokenData : RSDData
    {
        public string Token;
        public string Name;
        public string GamerTag;
        public string UnityId;
    }

    public class TokensRSD : RSD
    {
        #region Members

        public new static TokensRSD Instance => RSDManager.GetRSD<TokensRSD>();

        protected override string m_SheetId => "1xzYKzmTha3LlA2uX_gzLUpDEBurEmSz-2qsuKhsyoLk";
        protected override string m_SheetName => "Tokens";
        public override List<TokenData> Data { get; set; }

        #endregion


        #region Loading & Saving

        #endregion
        /// <summary>
        /// Read and store data received from the sheets
        /// </summary>
        /// <param name="data"></param>
        protected override void ReadSheetsData(SSheetsData data) { Data = FormatSheetData<TokenData>(data); }


        #region Tokens

        /// <summary>
        /// Check if token exists in list of authorized tokens
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsTokenAuthorized(string token)
        {
            foreach (TokenData data in Instance.Data)
            {
                if (token == data.Token)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if provided token can be used as new token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static bool IsTokenValid(string token, out string reason) 
        {
            reason = "";
            foreach (TokenData data in Instance.Data)
            {
                if (token == data.Token)
                {
                    if (data.UnityId != "")
                    {
                        reason = "Token has already been used";
                        return false;
                    }

                    return true;
                }
            }

            reason = "Invalid token";
            return false;
        }

        #endregion
    }
}