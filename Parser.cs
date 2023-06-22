namespace Lox;

class Parser
{
    private class ParseError : Exception { }
    private readonly Token[] _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        this._tokens = tokens.ToArray();
    }
    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.VAR))
            {
                return varDecl();
            }
            if (Match(TokenType.FUN))
            {
                return Function("function");
            }
            if (Match(TokenType.CLASS))
            {
                return ClassDeclaration();
            }
            return Statement();
        }
        catch (ParseError e)
        {
            Syncronise();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect class name.");

        Expr.VariableExpr superclass = null;
        if (Match(TokenType.LESS)){
            Token superclassName = Consume(TokenType.IDENTIFIER, "Expect identifier for superclass after '<'.");
            superclass = new Expr.VariableExpr(superclassName);
        }
        Consume(TokenType.LEFT_BRACE, "Expect '{' after class name.");
        List<Stmt.FunctionStmt> methods = new();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' at the end of class definition.");

        return new Stmt.Class_Stmt(name, superclass, methods);
    }

    private Stmt.FunctionStmt Function(string kind)
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect function name.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " identifier.");
        List<Token> parameters = Parameters();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after " + kind + " parameters.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' after " + kind + " parameters.");
        List<Stmt> bodySatements = Block();

        return new Stmt.FunctionStmt(name, parameters, bodySatements);
    }

    private List<Token> Parameters()
    {
        List<Token> parameters = new();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count() >= 255)
                {
                    Error(Previous(), "Maximum of 255 parameters exceeded");
                }
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }
        return parameters;
    }

    // varDecl -> "var" IDENTIFIER ( "=" expression)?;
    private Stmt varDecl()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
        Expr initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }
        Stmt stmt = new Stmt.VarStmt(name, initializer);
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return stmt;
    }

    private Stmt Statement()
    {
        if (Match(TokenType.PRINT))
        {
            return PrintStatement();
        }
        if (Match(TokenType.LEFT_BRACE))
        {
            return new Stmt.BlockStmt(Block());
        }
        if (Match(TokenType.IF))
        {
            return IfStatement();
        }
        if (Match(TokenType.WHILE))
        {
            return WhileStatement();
        }
        if (Match(TokenType.FOR))
        {
            return ForStatement();
        }
        if (Match(TokenType.RETURN))
        {
            return ReturnStatement();
        }
        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        Token returnKeyword = Previous();
        Expr expr = null;
        if (!Check(TokenType.SEMICOLON))
        {
            expr = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of return statement.");
        return new Stmt.Return_Stmt(returnKeyword, expr);
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
        Stmt initializer = Declaration();
        Expr condition = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' in 'for' after condition.");
        Stmt increment = new Stmt.ExpressionStmt(Expression());
        Consume(TokenType.RIGHT_PAREN, "Expect ')' at the end of 'for'.");
        Stmt body = Statement();

        //Desugaring for statements into while statements
        Stmt whileBody;
        if (body is Stmt.BlockStmt blockBody)
        {
            blockBody.statements.Add(increment);
            whileBody = blockBody;
        }
        else
        {
            whileBody = new Stmt.BlockStmt(new List<Stmt>() { body, increment });
        }
        Stmt whileStmt = new Stmt.While_Stmt(condition, whileBody);
        return new Stmt.BlockStmt(new List<Stmt>() { initializer, whileStmt });
    }

    private Stmt WhileStatement()
    {

        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'while' condition.");
        Stmt body = Statement();
        return new Stmt.While_Stmt(condition, body);

    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'if' condition.");
        Stmt thenBranch = Statement();
        Stmt elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }
        return new Stmt.If_Stmt(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new List<Stmt>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt ExpressionStatement()
    {
        Stmt stmt = new Stmt.ExpressionStmt(Expression());
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return stmt;
    }

    private Stmt PrintStatement()
    {
        Stmt stmt = new Stmt.PrintStmt(Expression());
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return stmt;
    }

    public Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.EQUAL))
        {
            Token equals = Previous();
            Expr value = Assignment();
            if (expr is Expr.VariableExpr varExpr)
            {
                return new Expr.AssignExpr(varExpr.name, value);
            }
            else if (expr is Expr.Get_Expr getExpr)
            {
                return new Expr.Set_Expr(getExpr.object_, getExpr.name, value);
            }
            throw Error(equals, "Invalid assignment target.");
        }
        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();
        while (Match(TokenType.OR))
        {
            Token token = Previous();
            Expr right = And();
            expr = new Expr.LogicalExpr(expr, token, right);
        }
        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();
        while (Match(TokenType.AND))
        {
            Token token = Previous();
            Expr right = Equality();
            expr = new Expr.LogicalExpr(expr, token, right);
        }
        return expr;
    }

    /**
     * 2 == 2 == 3
     *
     *
     *      == 
     *   ==    3
     * 2   2
     * ----------------
     * Eq -> Cmp (( "==" | "!=" ) Cmp)*
     * */
    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL))
        {
            Token oper = Previous();
            Expr second = Comparison();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    /*
     * Cmp -> Term ((< | > | <= | >=) Term)* 
     */
    private Expr Comparison()
    {

        Expr expr = Term();

        while (Match(TokenType.LESS, TokenType.GREATER, TokenType.LESS_EQUAL, TokenType.GREATER_EQUAL))
        {
            Token oper = Previous();
            Expr second = Term();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    private Expr Term()
    {

        Expr expr = Factor();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token oper = Previous();
            Expr second = Factor();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    private Expr Factor()
    {

        Expr expr = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token oper = Previous();
            Expr second = Unary();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.MINUS, TokenType.BANG))
        {
            Token token = Previous();
            Expr right = Call();
            return new Expr.UnaryExpr(token, right);
        }
        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();
        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.DOT))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Expr.Get_Expr(expr, name);
            }
            else break;

        }
        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> args = new();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (args.Count() >= 255)
                {
                    Error(Previous(), "Maximum of 255 arguments exceeded");
                }
                args.Add(Expression());
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Exprect ')' at the end of function call.");

        return new Expr.CallExpr(callee, Previous(), args);
    }


    /**
     * primary → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;
     */
    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new Expr.LiteralExpr(false);
        if (Match(TokenType.TRUE)) return new Expr.LiteralExpr(true);
        if (Match(TokenType.NIL)) return new Expr.LiteralExpr(null);
        if (Match(TokenType.THIS)) return new Expr.This_Expr(Previous());

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.LiteralExpr(Previous().Literal);
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.GroupingExpr(expr);
        }
        if (Match(TokenType.IDENTIFIER))
        {
            return new Expr.VariableExpr(Previous());
        }
        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType tokenType, string message)
    {
        if (Check(tokenType))
        {
            return Advance();
        }
        throw Error(Peek(), message);
    }

    private Exception Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }

    private void Syncronise()
    {
        Advance();

        while (!IsAtEnd())
        {
            switch (Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
            Advance();
        }
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) ++_current;
        return Previous();
    }

    private Token Peek()
    {
        return _tokens[_current];
    }
    private Token Previous()
    {
        return _tokens[_current - 1];
    }
    private Token PeekNext()
    {
        return _tokens[_current + 1];
    }

    internal List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }
        return statements;
    }
}

