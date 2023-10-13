public class Token
{
    //PALABRAS CLAVE
    public const string IF = "if"; //if
    public const string ELSE = "else"; //else
    public const string FUNCTION_DECLARATOR = "function"; //orden function
    public const string LET = "let"; //let
    public const string IN = "in"; //in
    public const string SPACE = " ";

    //LITERAL NUMERICOS
    public const string Number = "number"; //1,2,3,4,.. //NO SE SI PONERLOS ENUM U OTRA COSA YA QUE EN MODO CONSTANTE NO ME SIRVE TRABAJAR CON ELLOS

    //LITERAL DE CADENA
    public const string Text = "text";//"abcd.."  //NO SE SI PONERLOS ENUM U OTRA COSA YA QUE EN MODO CONSTANTE NO ME SIRVE TRABAJAR CON ELLOS

    //LITERALES BOOLEANOS
    public const string Boolean = "boolean";

    //EXPRESIONES ARITMETICAS
    public const string ADD = "+";
    public const string SUBSTRACT = "-";
    public const string MULTIPLY = "*";
    public const string SPLIT = "/";
    public const string EXPONENTIAL = "^";
    public const string REST = "%";

    //COMPARISON TOKENS
    public const string EQUIVALENT = "==";
    public const string MINOR = "<";
    public const string MAJOR = ">";
    public const string MINOR_E = "<=";
    public const string MAJOR_E = ">=";
    public const string DIFFERENT = "!=";
    public const string FUNCTION_CORPUS = "=>"; //=>
    public const string EQUAL = "="; //=
    public const string CONCAT = "@"; //@
    public const string AND = "&"; //&
    public const string OR = "|"; //|
    public const string NOT = "!"; //!

    //IDENTIFICADORES
    public const string REFERENCE = "reference"; //var_name o func_name

    //ARGUMENTOS
    public const string OPEN_1 = "("; //(
    public const string CLOSED_1 = ")"; //)
    public const string FOLLOW = ","; //,
    public const string CLOSE_LINE = ";"; //;
    public const char OP_EN_TEXT = '"';

    //CONSTANTES
    public const string PI = "number"; //PI
    public const string E = "number"; //E
    
}