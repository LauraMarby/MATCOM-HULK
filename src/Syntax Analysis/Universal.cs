public static class Universal
{
    //Aquí se guardan las funciones creadas con la herramienta function
    public static List<Function_type> Functions = new List<Function_type>();     

    //Aquí se guardan las variables declaradas en let-in con sus valores
    public static Dictionary<string, List<string>> Variables = new Dictionary<string, List<string>>();


    //Este método verifica si existe una función con el nombre name en la lista
    public static bool Is_Function_on_List(string name) 
    {
        foreach (Function_type function in Functions)
        {
            if (function.name == name)
            {
                return true;
            }
        }
        return false;
    }

    //Este método verifica si existe una variable con el nombre name en el diccionario
    public static bool Is_Variable_on_Dict(string name) 
    {
        if (Variables.ContainsKey(name))
        {
            return true;
        }
        return false;
    }

    //Este método limpia el diccionario de variables
    public static void Clean_Var()
    {
        Variables=new Dictionary<string, List<string>>();
    }

    //Este método elimina de la lista la función definida
    public static void Remove_Function(string name)
    {
        List<Function_type> Remove = new List<Function_type>();
        foreach (Function_type function in Functions)
        {
            if (function.name == name)
            {
                Remove.Add(function);
            }
        }
        foreach (Function_type function in Remove)
        {
            Functions.Remove(function);
        }
    }

    //Este método devuelve la función correspondiente
    public static Function_type Get_Func(string name)
    {
        foreach(Function_type f in Functions)
        {
            if(name==f.name)
            {
                return f;
            }
        }
        return new Function_type(new List<string>(), new List<string>());
    }
}