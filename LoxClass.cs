
class LoxClass {
  public String Name {get; init;}

  public LoxClass(String name) {
    this.Name = name;
  }

  override public String ToString() {
    return Name;
  }
}
