using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Framework.Components;

namespace ExampleProjectLib
{
    /// <summary>
    /// Gate
    /// </summary>
    class ExampleClient
    {
        private HandlerClient _gateClient;
        public ExampleClient()
        {
            _gateClient = new HandlerClient("127.0.0.1", 13001);
            
        }

        public void Start()
        {
            //_gateClient.Call<LoginResProto>()
        }

        PlayerClient Login()
        {
            return null;
        }
        
    }

    /// <summary>
    /// Game Server
    /// </summary>
    class PlayerClient
    {
        public PlayerClient()
        {
            SubcribeMe();
        }
        /// <summary>
        /// 订阅玩家信息有改动
        /// </summary>
        void SubcribeMe()
        {
            
        }

        public void GetNpcs()
        {
            
        }

        /// <summary>
        /// 每秒砍2下
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="skillId"></param>
        public void CastSkill(int npcId, int skillId)
        {
            
        }
    }
}
