using Friendica_Mobile.Viewmodels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Friendica_Mobile.Models
{
    public class FriendicaSampleData
    {
        public List<FriendicaThread> PrepareNetworkSamples()
        {
            var threads = new List<FriendicaThread>();
            var postsThread1 = new List<FriendicaPost>();
            var postsThread2 = new List<FriendicaPost>();

            var userPetyr = GetUserPetyr();
            var userJaime = GetUserJaime();
            var userTyrion = GetUserTyrion();
            var userArya = GetUserArya();
            var userSansa = GetUserSansa();
            var userCatelyn = GetUserCatelyn();
            var userDaenerys = GetUserDaenerys();
            var userMyself = new FriendicaUser();
            userMyself.IsAuthenticatedUser = true;

            // prepare first post sample
            string sample = "{\"text\":\"* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\nLorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\\n\\n* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\nhttps://www.youtube.com/watch?v=OACu7zCagi8\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:54 +0000 2015\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"Friendica\",\"id\":4,\"id_str\":\"4\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":{\"type\":\"Point\",\"coordinates\":[40.7127837,-74.0059413]},\"location\":\"New York\",\"favorited\":false,\"user\":{\"id\":1,\"id_str\":\"1\",\"name\":\"Testuser #1\",\"screen_name\":\"test1\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":2,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":1,\"network\":\"dfrn\"},\"statusnet_html\":\"<ul class=\\\"listbullet\\\" style=\\\"list-style-type: circle;\\\"><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li></ul><br><br>Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.<br><br><ul class=\\\"listdecimal\\\" style=\\\"list-style-type: decimal;\\\"><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li></ul><br><br><a href=\\\"https://www.youtube.com/watch?v=OACu7zCagi8\\\" target=\\\"_blank\\\">https://www.youtube.com/watch?v=OACu7zCagi8</a>\",\"statusnet_conversation_id\":\"4\"}";
            var jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post1 = new FriendicaPost(jsonPost);
            AddSampleUsers(post1, null, FriendicaActivity.like);
            AddSampleUsers(post1, userPetyr, FriendicaActivity.dislike);
            postsThread1.Add(post1);

            // prepare second post sample
            sample = "{\"text\":\"#include <iostream>\\nusing namespace std;\\nvoid main()\\n{\\ncout << \\\"Hello World!\\\" << endl;\\ncout << \\\"Welcome to C++ Programming\\\" << endl;\\n}\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:40 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":3,\"id_str\":\"3\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":3,\"id_str\":\"3\",\"name\":\"Testuser #3\",\"screen_name\":\"test3\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":3,\"network\":\"dfrn\"},\"statusnet_html\":\"<code>#include &lt;iostream&gt;<br>using namespace std;<br>void main()<br>{<br>     cout &lt;&lt; \\\"Hello World!\\\" &lt;&lt; endl;   <br>     cout &lt;&lt; \\\"Welcome to C++ Programming\\\" &lt;&lt; endl; <br>}</code>\",\"statusnet_conversation_id\":\"1\"}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post2 = new FriendicaPost(jsonPost);
            AddSampleUsers(post2, null, FriendicaActivity.like);
            AddSampleUsers(post2, null, FriendicaActivity.dislike);
            postsThread2.Add(post2);

            // prepare third post sample
            sample = "{\"text\":\"Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. :coffee\\n\\nMax Mustermann wrote:\\n> Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)\\n\\nhttps://de.wikipedia.org/wiki/Friendica\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:23 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":2,\"id_str\":\"2\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":2,\"id_str\":\"2\",\"name\":\"Testuser #2\",\"screen_name\":\"test2\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":2,\"network\":\"dfrn\"},\"statusnet_html\":\"Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. :coffee<br><br><br><strong class=\\\"author\\\">Max Mustermann wrote:</strong><blockquote>Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)</blockquote><br><br><a href=\\\"https://de.wikipedia.org/wiki/Friendica\\\" target=\\\"_blank\\\">https://de.wikipedia.org/wiki/Friendica</a>\",\"statusnet_conversation_id\":\"1\"}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post3 = new FriendicaPost(jsonPost);
            post3.IsNewEntry = true;
            AddSampleUsers(post3, userJaime, FriendicaActivity.like);
            AddSampleUsers(post3, userTyrion, FriendicaActivity.like);
            AddSampleUsers(post3, null, FriendicaActivity.dislike);
            post3.IsNewEntry = false;
            postsThread2.Add(post3);

            // prepare forth post sample
            sample = "{\"text\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:45 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":2,\"id_str\":\"2\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":4,\"id_str\":\"4\",\"name\":\"Testuser #4\",\"screen_name\":\"test4\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":4,\"network\":\"dfrn\"},\"statusnet_html\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"statusnet_conversation_id\":\"1\"}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post4 = new FriendicaPost(jsonPost);
            AddSampleUsers(post4, userArya, FriendicaActivity.like);
            AddSampleUsers(post4, userCatelyn, FriendicaActivity.like);
            AddSampleUsers(post4, userSansa, FriendicaActivity.like);
            AddSampleUsers(post4, userDaenerys, FriendicaActivity.dislike);
            AddSampleUsers(post4, userMyself, FriendicaActivity.like);
            post4.IsNewEntry = false;
            postsThread2.Add(post4);

            // prepare fifth post sample
            sample = "{\"text\":\"Heading\\n\\nLorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)\\n\\nhttps://upload.wikimedia.org/wikipedia/commons/thumb/1/1c/FuBK_testcard_vectorized.svg/1200px-FuBK_testcard_vectorized.svg.png\\n\\nUt enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure eprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\\n\\nhttp://europa.eu/about-eu/basic-information/symbols/anthem/european-anthem-2012.mp3\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:00 +0000 2015\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"Friendica\",\"id\":1,\"id_str\":\"1\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":1,\"id_str\":\"1\",\"name\":\"Testuser #1\",\"screen_name\":\"test1\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":2,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":1,\"network\":\"dfrn\"},\"statusnet_html\":\"<h4>\\nHeading\\n</h4>\\nLorem ipsum dolor sit amet, <u>consectetur</u> adipisici elit, sed eiusmod <strong>tempor incidunt</strong> ut labore et dolore magna aliqua. :-)<br><br><img src=\\\"https://upload.wikimedia.org/wikipedia/commons/thumb/1/1c/FuBK_testcard_vectorized.svg/1200px-FuBK_testcard_vectorized.svg.png\\\" alt=\\\"Bild/Foto\\\"><br><br>Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure <em>eprehenderit</em> in voluptate velit esse <span style=\\\"color: red;\\\">cillum dolore eu fugiat nulla pariatur.</span> Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.<br><br><a href=\\\"http://europa.eu/about-eu/basic-information/symbols/anthem/european-anthem-2012.mp3\\\" target=\\\"_blank\\\">http://europa.eu/about-eu/basic-information/symbols/anthem/european-anthem-2012.mp3</a>\",\"statusnet_conversation_id\":\"1\"}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post5 = new FriendicaPost(jsonPost);
            AddSampleUsers(post5, null, FriendicaActivity.like);
            AddSampleUsers(post5, null, FriendicaActivity.dislike);
            post5.IsNewEntry = false;
            postsThread2.Add(post5);

            var thread1 = new FriendicaThread();
            thread1.ThreadId = 1;
            thread1.MainPost.Add(post5);
            thread1.Comments.Add(post4);
            thread1.Comments.Add(post3);
            thread1.Comments.Add(post2);
            thread1.CollapseComments();
            thread1.IsThreadLoaded = true;

            var thread2 = new FriendicaThread();
            thread2.ThreadId = 4;
            thread2.MainPost.Add(post1);
            thread2.CollapseComments();
            thread2.IsThreadLoaded = true;

            threads.Add(thread2);
            threads.Add(thread1);

            return threads;
        }


        public List<FriendicaThread> PrepareNewsfeedSamples()
        {
            var threads = new List<FriendicaThread>();
            var postsThread1 = new List<FriendicaPost>();
            var postsThread2 = new List<FriendicaPost>();
            var postsThread3 = new List<FriendicaPost>();
            var postsThread4 = new List<FriendicaPost>();
            var postsThread5 = new List<FriendicaPost>();

            // prepare first post sample
            string sample = "{\"text\":\"Analysis: Everything is going great! In one Trump team photo\nhttp://edition.cnn.com/2017/08/20/politics/camp-david-pic/index.html\",\"truncated\":false,\"created_at\":\"Sun Aug 20 15:50:46 +0000 2017\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"coredev-bumblebee (RSS/Atom)\",\"id\":50831,\"id_str\":\"50831\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"favorited\":false,\"user\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"cid\":598,\"network\":\"feed\"},\"friendica_owner\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":false,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"uid\":2,\"cid\":598,\"self\":\"0\",\"network\":\"feed\"},\"statusnet_html\":\"Analysis: Everything is going great! In one Trump team photo<span class=\\\"type-link\\\"><a href=\\\"http://edition.cnn.com/2017/08/20/politics/camp-david-pic/index.html\\\" target=\\\"_blank\\\"><img src=\\\"http://i2.cdn.cnn.com/cnnnext/dam/assets/170820100216-camp-david-twitter-photo-super-tease.jpg\\\" alt =\\\"\\\" title=\\\"Everything is going great! in 1 amazing photo\\\" class=\\\"attachment-image\\\"></a><br>\n<a href=\\\"http://edition.cnn.com/2017/08/20/politics/camp-david-pic/index.html\\\" target=\\\"_blank\\\">Everything is going great! in 1 amazing photo</a>\n<blockquote>On Friday, President Donald Trump gathered more than a dozen of his top advisers at Camp David to discuss the future of the US military strategy in Afghanistan.</blockquote>\n</span>\",\"statusnet_conversation_id\":\"50831\",\"friendica_activities\":{\"like\":[],\"dislike\":[],\"attendyes\":[],\"attendno\":[],\"attendmaybe\":[]},\"attachments\":false}";
            var jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post1 = new FriendicaPost(jsonPost);
            post1.IsNewEntry = true;
            post1.PostType = PostTypes.Newsfeed;
            postsThread1.Add(post1);
            var thread1 = new FriendicaThread();
            thread1.ThreadId = 501;
            thread1.MainPost.Add(post1);
            thread1.CollapseComments();
            thread1.IsThreadLoaded = true;
            threads.Add(thread1);

            // prepare second post sample
            sample = "{\"text\":\"Battle for Tal Afar begins; civilians flee city\nhttp://edition.cnn.com/2017/08/20/middleeast/tal-afar-iraq-isis-assault/index.html\",\"truncated\":false,\"created_at\":\"Sun Aug 20 15:18:34 +0000 2017\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"coredev-bumblebee (RSS/Atom)\",\"id\":50823,\"id_str\":\"50823\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"favorited\":false,\"user\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 15:18:34 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"cid\":598,\"network\":\"feed\"},\"friendica_owner\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":false,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"uid\":2,\"cid\":598,\"self\":\"0\",\"network\":\"feed\"},\"statusnet_html\":\"Battle for Tal Afar begins; civilians flee city <span class=\\\"type-link\\\"><a href=\\\"http://edition.cnn.com/2017/08/20/middleeast/tal-afar-iraq-isis-assault/index.html\\\" target=\\\"_blank\\\"><img src=\\\"http://i2.cdn.cnn.com/cnnnext/dam/assets/170820101553-03-tal-afar-super-tease.jpg\\\" alt=\\\"\\\" title=\\\"Battle for Tal Afar begins as civilians flee Iraqi city\\\" class=\\\"attachment-image\\\"></a><br>\n<a href=\\\"http://edition.cnn.com/2017/08/20/middleeast/tal-afar-iraq-isis-assault/index.html\\\" target=\\\"_blank\\\">Battle for Tal Afar begins as civilians flee Iraqi city</a>\n<blockquote>The Iraqi army has begun its offensive to take the northwestern Iraqi city of Tal Afar from ISIS, Iraq's Prime Minister Haider al Abadi said Sunday.</blockquote>\n</span>\",\"statusnet_conversation_id\":\"50823\",\"friendica_activities\":{\"like\":[],\"dislike\":[],\"attendyes\":[],\"attendno\":[],\"attendmaybe\":[]},\"attachments\":false}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post5 = new FriendicaPost(jsonPost);
            post5.IsNewEntry = true;
            post5.PostType = PostTypes.Newsfeed;
            postsThread5.Add(post5);
            var thread5 = new FriendicaThread();
            thread5.ThreadId = 505;
            thread5.MainPost.Add(post5);
            thread5.CollapseComments();
            thread5.IsThreadLoaded = true;
            threads.Add(thread5);

            // prepare third post sample
            sample = "{\"text\":\"Zimbabwe's first lady home after alleged assault on model\nhttp://edition.cnn.com/2017/08/20/africa/grace-mugabe-in-zimbabwe/index.html\",\"truncated\":false,\"created_at\":\"Sun Aug 20 14:23:06 +0000 2017\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"coredev-bumblebee (RSS/Atom)\",\"id\":50825,\"id_str\":\"508250\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"favorited\":false,\"user\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"cid\":598,\"network\":\"feed\"},\"friendica_owner\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":false,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"uid\":2,\"cid\":598,\"self\":\"0\",\"network\":\"feed\"},\"statusnet_html\":\"Zimbabwe's first lady home after alleged assault on model <span class=\\\"type-link\\\"><a href=\\\"http://edition.cnn.com/2017/08/20/africa/grace-mugabe-in-zimbabwe/index.html\\\" target=\\\"_blank\\\"><img src=\\\"http://i2.cdn.cnn.com/cnnnext/dam/assets/170815113642-grace-mugabe-super-tease.jpg\\\" alt=\\\"\\\" title=\\\"Grace Mugabe returns to Zimbabwe after alleged assault\\\" class=\\\"attachment-image\\\"></a><br>\n<a href=\\\"http://edition.cnn.com/2017/08/20/africa/grace-mugabe-in-zimbabwe/index.html\\\" target=\\\"_blank\\\">Grace Mugabe returns to Zimbabwe after alleged assault</a>\n<blockquote>Zimbabwe's first lady Grace Mugabe returned home Sunday after facing allegations of assault in South Africa.</blockquote>\n</span>\",\"statusnet_conversation_id\":\"50825\",\"friendica_activities\":{\"like\":[],\"dislike\":[],\"attendyes\":[],\"attendno\":[],\"attendmaybe\":[]},\"attachments\":false}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post4 = new FriendicaPost(jsonPost);
            post4.IsNewEntry = true;
            post4.PostType = PostTypes.Newsfeed;
            postsThread4.Add(post4);
            var thread4 = new FriendicaThread();
            thread4.ThreadId = 504;
            thread4.MainPost.Add(post4);
            thread4.CollapseComments();
            thread4.IsThreadLoaded = true;
            threads.Add(thread4);

            // prepare forth post sample
            sample = "{\"text\":\"Sierra Leone mudslide survivors recall day mountain moved\nhttp://edition.cnn.com/2017/08/20/africa/sierra-leone-mudslide-survivors-grief/index.html\",\"truncated\":false,\"created_at\":\"Sun Aug 20 13:27:56 +0000 2017\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"coredev-bumblebee (RSS/Atom)\",\"id\":50827,\"id_str\":\"50827\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"favorited\":false,\"user\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"cid\":598,\"network\":\"feed\"},\"friendica_owner\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":false,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"uid\":2,\"cid\":598,\"self\":\"0\",\"network\":\"feed\"},\"statusnet_html\":\"Sierra Leone mudslide survivors recall day mountain moved <span class=\\\"type-link\\\"><a href=\\\"http://edition.cnn.com/2017/08/20/africa/sierra-leone-mudslide-survivors-grief/index.html\\\" target=\\\"_blank\\\"><img src=\\\"http://i2.cdn.cnn.com/cnnnext/dam/assets/170819135755-08-sierra-leone-relief-centre-super-tease.jpg\\\" alt=\\\"\\\" title=\\\"Sierra Leone survivors remember day the mountain moved\\\" class=\\\"attachment-image\\\"></a><br>\n<a href=\\\"http://edition.cnn.com/2017/08/20/africa/sierra-leone-mudslide-survivors-grief/index.html\\\" target=\\\"_blank\\\">Sierra Leone survivors remember day the mountain moved</a>\n<blockquote>\\\"It happened as if it was a dream,\\\" says Gabriel Fattah Manga, recalling the day his world came tumbling down around him, and his life changed forever.</blockquote>\n</span>\",\"statusnet_conversation_id\":\"50831\",\"friendica_activities\":{\"like\":[],\"dislike\":[],\"attendyes\":[],\"attendno\":[],\"attendmaybe\":[]},\"attachments\":false}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post3 = new FriendicaPost(jsonPost);
            post3.IsNewEntry = false;
            post3.PostType = PostTypes.Newsfeed;
            postsThread3.Add(post3);
            var thread3 = new FriendicaThread();
            thread3.ThreadId = 503;
            thread3.MainPost.Add(post3);
            thread3.CollapseComments();
            thread3.IsThreadLoaded = true;
            threads.Add(thread3);

            // prepare fifth post sample
            sample = "{\"text\":\"North Korea warns of 'merciless strike' ahead of drills\nhttp://edition.cnn.com/2017/08/20/asia/north-korea-south-korea-us-military-drills/index.html\",\"truncated\":false,\"created_at\":\"Sun Aug 20 10:03:48 +0000 2017\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"coredev-bumblebee (RSS/Atom)\",\"id\":50829,\"id_str\":\"50829\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"favorited\":false,\"user\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"cid\":598,\"network\":\"feed\"},\"friendica_owner\":{\"id\":599,\"id_str\":\"599\",\"name\":\"CNN.com - RSS Channel - App International Edition\",\"screen_name\":\"cnn.com\",\"location\":\"RSS/Atom\",\"description\":null,\"profile_image_url\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"profile_image_url_https\":\"http://i2.cdn.turner.com/cnn/2015/images/09/24/cnn.digital.png\",\"url\":\"http://rss.cnn.com/rss/edition.rss\",\"protected\":false,\"followers_count\":0,\"friends_count\":0,\"listed_count\":0,\"created_at\":\"Sun Aug 20 20:37:03 +0000 2017\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"geo_enabled\":false,\"verified\":true,\"statuses_count\":0,\"lang\":\"\",\"contributors_enabled\":false,\"is_translator\":false,\"is_translation_enabled\":false,\"following\":false,\"follow_request_sent\":false,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"http://rss.cnn.com/rss/edition.rss\",\"uid\":2,\"cid\":598,\"self\":\"0\",\"network\":\"feed\"},\"statusnet_html\":\"North Korea warns of 'merciless strike' ahead of drills <span class=\\\"type-link\\\"><a href=\\\"http://edition.cnn.com/2017/08/20/asia/north-korea-south-korea-us-military-drills/index.html\\\" target=\\\"_blank\\\">North Korea warns of 'merciless strike' ahead of US-South Korea drills</a>\n<blockquote>North Korea warned Sunday that the upcoming US-South Korea military exercises are \\\"reckless behavior driving the situation into the uncontrollable phase of a nuclear war.\\\"</blockquote>\n</span>\",\"statusnet_conversation_id\":\"50831\",\"friendica_activities\":{\"like\":[],\"dislike\":[],\"attendyes\":[],\"attendno\":[],\"attendmaybe\":[]},\"attachments\":false}";
            jsonPost = JsonConvert.DeserializeObject<JsonFriendicaPost>(sample);
            var post2 = new FriendicaPost(jsonPost);
            post2.IsNewEntry = false;
            post2.PostType = PostTypes.Newsfeed;
            postsThread2.Add(post2);
            var thread2 = new FriendicaThread();
            thread2.ThreadId = 502;
            thread2.MainPost.Add(post2);
            thread2.CollapseComments();
            thread2.IsThreadLoaded = true;
            threads.Add(thread2);

            return threads;
        }


        private FriendicaUser GetUserPetyr()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "P",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 9,
                    UserName = "Petyr Baelish",
                    UserUrl = "https://friendica.server.text/profile/petyr",
                    UserScreenName = "petyr",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/5/5e/Aidan_Gillen_as_Petyr_Baelish.jpg"
                }
            };
            return user;
        }


        private FriendicaUser GetUserJaime()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "J",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 8,
                    UserName = "Jaime Lannister",
                    UserUrl = "https://friendica.server.text/profile/jaimeL",
                    UserScreenName = "jaimeL",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/b/b5/JaimeLannister.jpg"
                }
            };
            return user;
        }


        private FriendicaUser GetUserTyrion()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "T",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 1,
                    UserName = "Tyrion Lannister",
                    UserUrl = "https://friendica.server.text/profile/tyrion",
                    UserScreenName = "tyrion",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/5/50/Tyrion_Lannister-Peter_Dinklage.jpg"
                }
            };
            return user;
        }


        private FriendicaUser GetUserArya()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "A",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 6,
                    UserName = "Arya Stark",
                    UserUrl = "https://friendica.server.text/profile/aryastark",
                    UserScreenName = "aryastark",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg"
                }
            };
            return user;
        }


        private FriendicaUser GetUserCatelyn()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "C",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 10,
                    UserName = "Catelyn Stark",
                    UserUrl = "https://friendica.server.text/profile/catelyn",
                    UserScreenName = "catelyn",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/1/1b/Catelyn_Stark_S3.jpg"
                }
            };
            return user;
        }


        private FriendicaUser GetUserSansa()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "S",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 5,
                    UserName = "Sansa Stark",
                    UserUrl = "https://friendica.server.text/profile/sansa",
                    UserScreenName = "sansa",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/7/74/SophieTurnerasSansaStark.jpg"
                }
            };
            return user;
        }


        private FriendicaUser GetUserDaenerys()
        {
            var user = new FriendicaUser()
            {
                IsAuthenticatedUser = false,
                CharacterGroup = "D",
                ContactType = ContactTypes.Friends,
                User = new JsonFriendicaUser()
                {
                    UserCid = 3,
                    UserName = "Daenerys Targaryen",
                    UserUrl = "https://friendica.server.text/profile/daenerys",
                    UserScreenName = "daenerys",
                    UserProfileImageUrl = "https://upload.wikimedia.org/wikipedia/en/0/0d/Daenerys_Targaryen_with_Dragon-Emilia_Clarke.jpg"
                }
            };
            return user;
        }


        private void AddSampleUsers(FriendicaPost post, FriendicaUser user, FriendicaActivity activity)
        {
            post.Post.PostFriendicaActivities = new JsonFriendicaActivities();

            if (post.Activities == null)
                post.Activities = new FriendicaActivities();

            if (user != null)
            {
                if (activity == FriendicaActivity.like)
                {
                    post.Activities.ActivitiesLike.Add(user);
                }
                else if (activity == FriendicaActivity.dislike)
                {
                    post.Activities.ActivitiesDislike.Add(user);
                }
            }

            post.SetActivitiesParameters();
        }

    }
}
