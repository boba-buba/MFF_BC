using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
// IMPORTANT NOTE: You should NOT change anything in Program class
class Program {
    static RequestProcessor requestProcessor = new RequestProcessor();

    static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        requestProcessor.RegisterAllRoutes();

        app.Run(ProcessRequest);
        app.UseHttpsRedirection();

        app.Run();
    }

    static Task ProcessRequest(HttpContext context) {
        var path = context.Request.Path.Value ?? "";
        var query = context.Request.QueryString.Value ?? "";
        var result = requestProcessor.HandleRequest(path, query);
        return context.Response.WriteAsync(result);
    }
}

/// <summary>
/// Class represents router of the requests.
/// </summary>
class RequestProcessor {

    /// <summary>
    /// Map, that holds possible routes
    /// </summary>
    private RouteMap _routes = new RouteMap();


    /// <summary>
    /// Finds all handlers in assembly and registers their functions in map
    /// to process requests with those functions later.
    /// </summary>
    public void RegisterAllRoutes() {
        Console.WriteLine("++++++Finding types");

        // TODO: Find all classes implementing ISimplisticRoutesHandler in the calling assembly
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsAssignableTo(typeof(ISimplisticRoutesHandler)) && !type.IsInterface);

        // TODO: Create instance of each such class using parameter-less constructor
        // TODO: Call RegisterRoutes method on each such instance
        foreach (var type in types)
        {
            var instance = (ISimplisticRoutesHandler)Activator.CreateInstance(type);
            instance.RegisterRoutes(_routes);
        }
	}


    /// <summary>
    /// Calls corresponding function according to request (path + query)
    /// </summary>
    /// <param name="path">Path of the request</param>
    /// <param name="query">The query with parameters</param>
    /// <returns>result of the request</returns>
	public string HandleRequest(string path, string query) {
        Console.WriteLine($"+++ Thread #{Thread.CurrentThread.ManagedThreadId} processing request:");
        Console.WriteLine($"    Path =\"{path}\"");
        Console.WriteLine($"    Query=\"{query}\" ...");
 
		// TODO: Replace the code below with your own request processing (do not remove the logging above):
        if (_routes.TryGetRoute(path, out Delegate foundRoute))
        {
            // TODO: Pass correct arguments to requested service method (ignore additinal parameters in query, that the service method does not have)
            //       Parameter name matching should be case-insensitive!

            var methodParams = foundRoute.GetMethodInfo().GetParameters();
            var actualParams = GetParameters(query);

            if (methodParams.Length == 0)
            {
                return foundRoute.DynamicInvoke().ToString();
            }
            List<string> values = new List<string>();
            if (!ParamsOK(methodParams, actualParams, out values))
            {
                return "{}";
            }  
            dynamic r = foundRoute.DynamicInvoke(values.ToArray());
            return PrettyPrint(r);
        }
        else
        {
    		// TODO: Check the route is valid - if not print the following:
            Console.WriteLine($"!!! Route not found !!!");
        }

        return "{}";
    }


    private string PrettyPrint(dynamic result)
    {
        StringBuilder sb = new StringBuilder();
        if (result.GetType() == typeof(PostCodeItem[]))
        {
            foreach (var el in result) { sb.AppendLine(el.ToString()); }
            return sb.ToString();
        }
        return result.ToString();
    }

    /// <summary>
    /// Checks if the actual parameters from query respond to the parameters of the function
    /// </summary>
    /// <param name="expected">Parameters of function</param>
    /// <param name="actual">Parameters read from query</param>
    /// <param name="values">Placeholder for actual values of parameters</param>
    /// <returns>true, if actual parameters correspond to expected, false otherwise</returns>
    private bool ParamsOK(ParameterInfo[] expected, IReadOnlyDictionary<string, string> actual, out List<string> values)
    {
        bool ok = true;
        values = new List<string>();
        foreach (var param in expected)
        {
            if (!actual.ContainsKey(param.Name))
            {
                // TODO: Check all parameters for the service method are part of the query - if not print following (for specific parameter name):
                Console.WriteLine($"!!! Parameter {param.Name} is missing !!!");
                ok = false;
            }
            else
            {
                ok = ok | true;
                values.Add(actual[param.Name]);
            }
        }
        return ok;
    }


    /// <summary>
    /// Reads parameters from actual query and parses them.
    /// </summary>
    /// <param name="query">Query to read parameters from</param>
    /// <returns>Dictionary with {name, value} pairs of parameters </returns>
    private IReadOnlyDictionary<string, string> GetParameters(string query)
    {
        var parameters = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(query)) 
            return parameters;

        var pars = query.Substring(1).Split('&');
        foreach ( var par in pars )
        {
            var pair = par.Split('=');
            StringBuilder builder = new StringBuilder(pair[1]);
            // TODO: Interpret %20 and + as space in query string
            builder.Replace("+", " ");
            builder.Replace("%20", " ");
            parameters.Add(pair[0], builder.ToString());
        }
        return parameters;
    }
}

/// <summary>
/// Map to store and retrieve functions to process query base on path
/// </summary>
public class RouteMap {
	// TODO: Add your implementation here:
	//		 You need to add at least a suitable Map method here - see example usage in PostRoutesHandler.cs

    /// <summary>
    /// Dictionary taht stores routes and function that process requests base on those routes.
    /// </summary>
    private Dictionary<string, Delegate> _routes = new Dictionary<string, Delegate>();


    /// <summary>
    /// Add new function into map
    /// </summary>
    /// <param name="path"></param>
    /// <param name="action"></param>
    public void Map(string path, Delegate action)
    {
        _routes.Add(path, action);
    }


    /// <summary>
    /// Tries to get function to process query
    /// </summary>
    /// <param name="path">Path to find function</param>
    /// <param name="value">Placeholder for function, null if no such path in map</param>
    /// <returns></returns>
    public bool TryGetRoute(string path, out Delegate value)
    {
        return _routes.TryGetValue(path, out value);
    }
}

// TODO: Add additional types if necessary

// IMPORTANT NOTE: You should NOT change anything in this interface
public interface ISimplisticRoutesHandler {
    public void RegisterRoutes(RouteMap routeMap);
} 

