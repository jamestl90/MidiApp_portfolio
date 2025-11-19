using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.Percent;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace TestApp
{
    public class CustomItemPicker : FrameLayout, View.IOnTouchListener, TextView.IOnEditorActionListener
    {
        public class CustomItem
        {
            public int Index;
            public string Text;
        }

        public class ItemPickerValueChangedEventArgs : EventArgs
        {
            public CustomItem Item { get; set; }
        }

        public EventHandler<ItemPickerValueChangedEventArgs> OnValueChanged;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private bool m_init = false;
        private Context m_context;

        private Button m_leftButton;
        private Button m_rightButton;

        private TextView m_textView;
        private EditText m_editText;

        private List<CustomItem> m_items;

        private bool m_isHolding = false;

        public bool IsDataNumeric { get; set; }

        public bool Editable { get; set; }

        public List<CustomItem> Items
        {
            get { return m_items; }
            set
            {
                m_items = value;
                OnItemsChanged();
            }
        }

        private int m_currentItem;

        public int CurrentItem
        {
            get { return m_currentItem; }
            set
            {
                m_currentItem = value;
                OnCurrentItemChanged();
            }
        }

        #region ctor
        public CustomItemPicker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomItemPicker(Context context) : base(context)
        {
            Initialise(context, null);
        }

        public CustomItemPicker(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialise(context, attrs);
        }

        public CustomItemPicker(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialise(context, attrs);
        }

        public CustomItemPicker(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialise(context, attrs);
        }
#endregion

        private void Initialise(Context ctx, IAttributeSet attrs)
        {
            if (!m_init)
            {
                m_context = ctx;

                if (attrs != null)
                {
                    TypedArray a = m_context.ObtainStyledAttributes(attrs, Resource.Styleable.CustomItemPicker);
                    IsDataNumeric = a.GetBoolean(Resource.Styleable.CustomItemPicker_IsDataNumeric, true);
                    a.Recycle();
                }

                LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
                View view = inflater.Inflate(Resource.Layout.CustomItemPickerLayout, this);

                m_leftButton = view.FindViewById<Button>(Resource.Id.leftButton);
                m_rightButton = view.FindViewById<Button>(Resource.Id.rightButton);
                m_textView = view.FindViewById<TextView>(Resource.Id.textView);
                m_editText = view.FindViewById<EditText>(Resource.Id.editText);

                m_editText.TextChanged += EditTextOnTextChanged;
                m_editText.SetOnEditorActionListener(this);

                m_leftButton.SetOnTouchListener(this);
                m_rightButton.SetOnTouchListener(this);

                m_textView.Click += TextViewClicked;

                //Items = DefaultItems();
                //CurrentItem = 9;

                m_init = true;
                Editable = false;
            }
        }

        private void EditTextOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {            
            //var value = textChangedEventArgs.Text.ToString();

            //int n;
            //bool isNumeric = int.TryParse(value, out n);

            //if (IsDataNumeric)
            //{
            //    if (!isNumeric)
            //    {
            //        // error!
            //        return;
            //    }
            //}
            //else
            //{
            //    if (isNumeric)
            //    {
            //        // error!
            //        return;
            //    }
            //}

            //foreach (var item in m_items)
            //{
            //    if (item.Text == value)
            //    {
            //        m_currentItem = item.Index;
            //        UpdateText();
            //        OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });
            //    }
            //}
        }

        public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Done)
            {
                ViewSwitcher switcher = (ViewSwitcher)FindViewById(Resource.Id.my_switcher);
                switcher.ShowNext(); //or switcher.showPrevious();

                var value = v.Text;

                int n;
                bool isNumeric = int.TryParse(value, out n);

                if (IsDataNumeric)
                {
                    if (!isNumeric)
                    {
                        // error!
                        return true;
                    }
                }
                else
                {
                    if (isNumeric)
                    {
                        // error!
                        return true;
                    }
                }

                foreach (var item in m_items)
                {
                    if (item.Text == value)
                    {
                        m_currentItem = item.Index;
                        UpdateText();
                        OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });
                    }
                }

                InputMethodManager imm = (InputMethodManager)((Activity)m_context).GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(m_editText.WindowToken, 0);

                return true;
            }
            return false;
        }

        public void TextViewClicked(object sender, EventArgs e)
        {
            if (!Editable) return;

            ViewSwitcher switcher = (ViewSwitcher)FindViewById(Resource.Id.my_switcher);
            switcher.ShowNext(); //or switcher.showPrevious();
        }

        public async Task StartTask(Action uiMethod, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                int initialDelay = 200;
                int count = 0;
                while (m_isHolding)
                {
                    count++;
                    await Task.Delay(initialDelay, cancellationToken);
                    ((Activity)m_context).RunOnUiThread(uiMethod);
                    //OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    if (count % 3 == 0)
                    {
                        if (initialDelay > 50)
                        {
                            initialDelay -= 50;
                        }
                    }
                }                
            }, cancellationToken);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (v.Id)
            {
                case Resource.Id.leftButton:
                    switch (e.Action)
                    {
                        case MotionEventActions.Down:
                            if (m_isHolding)
                                return false;

                            DecrementCurrent();
                            OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });

                            m_isHolding = true; 
                            StartTask(DecrementCurrent, _cts.Token);

                            return false;
                        case MotionEventActions.Up:

                            m_isHolding = false;
                            _cts.Cancel();
                            _cts = new CancellationTokenSource();
                            OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });
                            return false;
                    }

                    break;
                case Resource.Id.rightButton:
                    switch (e.Action)
                    {
                        case MotionEventActions.Down:
                            if (m_isHolding)
                                return false;

                            IncrementCurrent();
                            OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });

                            m_isHolding = true;                            
                            StartTask(IncrementCurrent, _cts.Token);

                            return false;
                        case MotionEventActions.Up:

                            _cts.Cancel();
                            _cts = new CancellationTokenSource();
                            m_isHolding = false;
                            OnValueChanged?.Invoke(this, new ItemPickerValueChangedEventArgs { Item = m_items.First(x => x.Index == CurrentItem) });
                            return false;
                    }

                    break;
                default:
                    return false;
            }
            return true;
        }

        private void OnCurrentItemChanged()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            m_textView.Text = m_items.First(x => x.Index == m_currentItem).Text;
        }

        private void DecrementCurrent()
        {
            --m_currentItem;
            if (m_currentItem == -1)
            {
                m_currentItem = m_items.Count - 1;
            }
            UpdateText();
        }

        private void IncrementCurrent()
        {
            ++m_currentItem;
            if (m_currentItem == m_items.Count)
            {
                m_currentItem = 0;
            }
            UpdateText();
        }

        private void OnItemsChanged()
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(MeasuredWidth, MeasuredHeight);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return base.OnTouchEvent(e);
        }
    }
}