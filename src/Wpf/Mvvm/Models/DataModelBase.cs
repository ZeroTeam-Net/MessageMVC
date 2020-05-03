// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

namespace Agebull.EntityModel
{
    /// <summary>
    ///     MVVM的Model的基类
    /// </summary>
    public abstract class DataModelBase : MvvmBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            DoInitialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void DoInitialize()
        {

        }
    }
}
