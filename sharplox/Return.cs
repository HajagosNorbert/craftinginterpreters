class Return : Exception {
    public object Value {get; init;}

    public Return(object value){
        Value = value;
    }
}
