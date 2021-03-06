﻿/* 
 * PROJECT: NyARToolkitCS
 * --------------------------------------------------------------------------------
 *
 * The NyARToolkitCS is C# edition NyARToolKit class library.
 * Copyright (C)2008-2012 Ryo Iizuka
 *
 * This work is based on the ARToolKit developed by
 *   Hirokazu Kato
 *   Mark Billinghurst
 *   HITLab, University of Washington, Seattle
 * http://www.hitl.washington.edu/artoolkit/
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as publishe
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * For further information please contact.
 *	http://nyatla.jp/nyatoolkit/
 *	<airmail(at)ebony.plala.or.jp> or <nyatla(at)nyatla.jp>
 * 
 */
using System.Diagnostics;
namespace jp.nyatla.nyartoolkit.cs.core
{







    /**
     * このクラスは、参照カウンタ付きのObjectPoolの基本クラスです。
     * {@link NyARManagedObject}から派生したオブジェクト型Tのオブジェクトプールを実現します。
     * 
     * オブジェクトプールは、未割当リストを使って、オブジェクト要素を管理します。
     * はじめ、全てのオブジェクト要素は未割当リストにあります。オブジェクト要素の生成要求があると、オブジェクトプール
     * は未割当リストにあるオブジェクト要素の参照カウンタを1加算して、未割当リストから削除します。
     * 逆に、オブジェクト要素の解放を要求があると、オブジェクト要素の参照カウンタを1減算して、その数が　0になったら、
     * オブジェクト要素を未割当リストへ加えます。
     * <br/>
     * このクラスは、NyARManagedObjectと密接に関係して動作することに注意してください。
     * 割り当て関数はオブジェクトプールの{@link NyARManagedObjectPool#newObject}、解放関数にはオブジェクト要素の
     * {@link NyARManagedObject#releaseObject()}を使います。
     * <br/>
     * また、このクラスは{@link NyARManagedObject}に対して、特殊な関数を持つインタフェイスを提供します。
     * このインタフェイスは、{@link NyARManagedObject}が{@link NyARManagedObjectPool}のリストを操作するために使います。
     * <p>継承クラスの実装について - 
     * このクラスは、そのまま実体化できません。継承クラスを実装する必要があります。
     * 実装するには、{@link #createElement}をオーバライドして、コンストラクタから{@link #initInstance}を呼び出します。
     * {@link #initInstance}には２種類の関数があります。要素生成パラメータの有無で、どちらかを選択して呼び出してください。
     * </p>
     * 
     * @param <T>
     */
    public class NyARManagedObjectPool<T> where T : NyARManagedObject
    {
        /**
         * このクラスは、{@link NyARManagedObject}へ提供する操作インタフェイスの実体です。
         * 未割当リストを操作する関数を定義します。
         * Javaの都合でバッファを所有させていますが、別にこの形で実装しなくてもかまいません。
         */
        public class Operator : NyARManagedObject.INyARManagedObjectPoolOperater
        {
            /** 要素の実体の保管用リスト*/
            public NyARManagedObject[] _buffer;
            /** 未割当オブジェクトのリスト*/
            public NyARManagedObject[] _pool;
            public int _pool_stock;
            public void deleteObject(NyARManagedObject i_object)
            {
                Debug.Assert(i_object != null);
                Debug.Assert(this._pool_stock < this._pool.Length);
                this._pool[this._pool_stock] = i_object;
                this._pool_stock++;
            }
        }
        /**
         * オブジェクト要素から参照する操作インタフェイス。ユーザが使用することはありません。
         * <p>メモ - このスコープはprotectedではないか？</p>
         */
        public Operator _op_interface = new Operator();

        /**
         * この関数は、新しいオブジェクトを１個割り当てて返します。
         * @return
         * 新しいオブジェクト。失敗した場合はnull
         */
        public T newObject()
        {
            Operator pool = this._op_interface;
            if (pool._pool_stock < 1)
            {
                return null;
            }
            pool._pool_stock--;
            //参照オブジェクトを作成して返す。
            return (T)(pool._pool[pool._pool_stock].initObject());
        }
        /**
         * コンストラクタです。
         * 生成拒否の為に、コンストラクタを隠蔽します。
         * 継承クラスを作成してください。
         */
        protected NyARManagedObjectPool()
        {
        }
        /**
         * この関数は、インスタンスを初期化します。
         * 継承クラスのコンストラクタから呼び出します。
         * {@link #initInstance(int, Class, Object)}との違いは、オブジェクトの生成に引数を渡すかどうかです。
         * 引数が必要な時は、こちらの関数を使って、{@link #createElement()}をオーバライドします。
         * @param i_length
         * 配列の最大長さ
         * @param i_element_type
         * 配列型を示すクラスタイプ
         * @
         */
        protected void initInstance(int i_length)
        {
            Operator pool = this._op_interface;
            //領域確保
            pool._buffer = new T[i_length];
            pool._pool = new T[i_length];
            //使用中個数をリセット
            pool._pool_stock = i_length;
            //オブジェクトを作成
            for (int i = pool._pool.Length - 1; i >= 0; i--)
            {
                pool._buffer[i] = pool._pool[i] = createElement();
            }
            return;
        }
        /**
         * この関数は、インスタンスを初期化します。
         * 継承クラスのコンストラクタから呼び出します。
         * {@link #initInstance(int, Class)}との違いは、オブジェクトの生成に引数を渡すかどうかです。
         * 引数が必要な時は、こちらの関数を使って、{@link #createElement(Object)}をオーバライドします。
         * @param i_length
         * 配列の最大長さ
         * @param i_element_type
         * 配列型を示すクラスタイプ
         * @param i_param
         * 配列要素を生成するときに渡すパラメータ
         * @
         */
        protected void initInstance(int i_length,object i_param)
        {
            Operator pool = this._op_interface;
            //領域確保
            pool._buffer =new T[i_length];
            pool._pool = new T[i_length];
            //使用中個数をリセット
            pool._pool_stock = i_length;
            //オブジェクトを作成
            for (int i = pool._pool.Length - 1; i >= 0; i--)
            {
                pool._buffer[i] = pool._pool[i] = createElement(i_param);
            }
            return;
        }
        /**
         * この関数は、配列要素のオブジェクトを１個作ります。
         * {@link #initInstance(int, Class)}から呼び出されます。
         * 継承クラスでオーバライドして、要素オブジェクトを１個生成して返す処理を実装してください。
         * @return
         * 新しいオブジェクトを返してください。
         * @
         */
        protected virtual T createElement()
        {
            throw new NyARRuntimeException();
        }
        /**
         * この関数は、配列要素のオブジェクトを(引数付きで)１個作ります。
         * {@link #initInstance(int, Class, object)}から呼び出されます。
         * 継承クラスでオーバライドして、要素オブジェクトを１個生成して返す処理を実装してください。
         * @return
         * 新しいオブジェクトを返してください。
         * @
         */
        protected virtual T createElement(object i_param)
        {
            throw new NyARRuntimeException();
        }
    }
}
