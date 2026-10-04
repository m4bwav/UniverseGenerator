#:property PublishAot=false
using System.Reflection;
// TSV: typeFullName \t typeKind \t memberKind \t signature \t xmlDocId
var asm = Assembly.LoadFrom(args[0]);
const BindingFlags F = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
string[] skip = { "Equals", "GetHashCode", "ToString", "PrintMembers", "Deconstruct", "<Clone>$" };
foreach (var t in asm.GetExportedTypes().OrderBy(t => t.FullName))
{
    var kind = t.IsEnum ? "enum" : t.IsInterface ? "interface" : t.IsValueType ? "struct" : (t.IsAbstract && t.IsSealed) ? "static class" : t.GetMethod("<Clone>$") != null ? "record" : "class";
    W(t, kind, "type", t.Name, "T:" + t.FullName);
    if (t.IsEnum) { foreach (var n in Enum.GetNames(t)) W(t, kind, "value", n, $"F:{t.FullName}.{n}"); continue; }
    foreach (var c in t.GetConstructors().Where(c => !(c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == t)))
        W(t, kind, "ctor", $"new {t.Name}({Ps(c)})", $"M:{t.FullName}.#ctor{Ids(c)}");
    foreach (var p in t.GetProperties(F).Where(p => p.Name != "EqualityContract"))
        W(t, kind, "property", $"{(p.GetMethod?.IsStatic == true ? "static " : "")}{N(p.PropertyType)} {p.Name}{(p.GetIndexParameters().Length > 0 ? "[" + string.Join(", ", p.GetIndexParameters().Select(x => N(x.ParameterType))) + "]" : "")}{(p.SetMethod?.IsPublic == true ? " { get; init; }" : "")}", $"P:{t.FullName}.{p.Name}");
    foreach (var f in t.GetFields(F))
        W(t, kind, "field", $"{(f.IsStatic ? "static " : "")}{N(f.FieldType)} {f.Name}", $"F:{t.FullName}.{f.Name}");
    foreach (var m in t.GetMethods(F).Where(m => !m.IsSpecialName && !skip.Contains(m.Name)))
        W(t, kind, "method", $"{(m.IsStatic ? "static " : "")}{N(m.ReturnType)} {m.Name}({Ps(m)})", $"M:{t.FullName}.{m.Name}{Ids(m)}");
}
static void W(Type t, string k, string mk, string sig, string id) => Console.WriteLine($"{t.FullName}\t{k}\t{mk}\t{sig}\t{id}");
static string Ps(MethodBase m) => string.Join(", ", m.GetParameters().Select(p => $"{(m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute)) && p.Position == 0 ? "this " : "")}{N(p.ParameterType)} {p.Name}{(p.HasDefaultValue ? " = " + Def(p.DefaultValue) : "")}"));
static string Def(object? v) => v is null ? "null" : v is bool b ? (b ? "true" : "false") : v is string s ? $"\"{s}\"" : v.ToString()!;
static string Ids(MethodBase m) { var ps = m.GetParameters(); return ps.Length == 0 ? "" : "(" + string.Join(",", ps.Select(p => Id(p.ParameterType))) + ")"; }
static string Id(Type t)
{
    if (t.IsByRef) return Id(t.GetElementType()!) + "@";
    if (t.IsArray) return Id(t.GetElementType()!) + "[]";
    if (t.IsGenericParameter) return (t.DeclaringMethod != null ? "``" : "`") + t.GenericParameterPosition;
    if (t.IsGenericType) { var d = t.GetGenericTypeDefinition().FullName!; return d.Substring(0, d.IndexOf('`')) + "{" + string.Join(",", t.GetGenericArguments().Select(Id)) + "}"; }
    return t.FullName!.Replace('+', '.');
}
static string N(Type t)
{
    if (Nullable.GetUnderlyingType(t) is { } u) return N(u) + "?";
    if (t.IsArray) return N(t.GetElementType()!) + "[]";
    var map = new Dictionary<Type, string> { [typeof(int)] = "int", [typeof(long)] = "long", [typeof(ulong)] = "ulong", [typeof(double)] = "double", [typeof(float)] = "float", [typeof(bool)] = "bool", [typeof(string)] = "string", [typeof(void)] = "void", [typeof(object)] = "object", [typeof(byte)] = "byte", [typeof(uint)] = "uint" };
    if (map.TryGetValue(t, out var s)) return s;
    return t.IsGenericType ? t.Name.Split('`')[0] + "<" + string.Join(", ", t.GetGenericArguments().Select(N)) + ">" : t.Name;
}
