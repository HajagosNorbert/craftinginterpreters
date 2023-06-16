using System.Runtime.Serialization;

namespace Lox;

class Parser
{
    private class ParseError : Exception
    {
        // public ParseError()
        // {
        // }
        //
        // public ParseError(string? message) : base(message)
        // {
        // }
        //
        // public ParseError(string? message, Exception? innerException) : base(message, innerException)
        // {
        // }
        //
        // protected ParseError(SerializationInfo info, StreamingContext context) : base(info, context)
        // {
        // }
    }
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
            return Statement();
        }
        catch (ParseError e)
        {
            Syncronise();
            return null;
        }
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
        return ExpressionStatement();
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
        if (Match(TokenType.ELSE)){
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

    /*
     * Assign -> IDENTIFIER "=" Assign | Eq
     * My approach is different.
     */
    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.EQUAL))
        {
            if (expr is Expr.VariableExpr varExpr)
            {
                return new Expr.AssignExpr(varExpr.name, Assignment());
            }
            else
            {
                throw Error(Previous(), "Invalid assignment target.");
            }
        }
        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();
        while (Match (TokenType.OR)){
            Token token = Previous();
            Expr right = And();
            expr = new Expr.LogicalExpr(expr, token, right);
        }
        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();
        while (Match (TokenType.AND)){
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

    /*
     *( "!" | "-" ) unary | primary
     */
    private Expr Unary()
    {
        if (Match(TokenType.MINUS, TokenType.BANG))
        {
            Token token = Previous();
            Expr right = Unary();
            return new Expr.UnaryExpr(token, right);
        }
        return Primary();
    }


    /**
     * primary → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;
     */
    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new Expr.LiteralExpr(false);
        if (Match(TokenType.TRUE)) return new Expr.LiteralExpr(true);
        if (Match(TokenType.NIL)) return new Expr.LiteralExpr(null);

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

