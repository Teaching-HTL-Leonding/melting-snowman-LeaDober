using System.Collections.Concurrent;
using System.Text.Json;
using MeltingSnowman.Logic;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>{
    options.SwaggerDoc("v1",new(){Title="Melting Snowman API ",Version="v1"});
});
var app = builder.Build();

var Games = new ConcurrentDictionary <int,MeltingSnowmanGame>();
Games.TryAdd(1,new MeltingSnowmanGame());
Games.TryAdd(2,new MeltingSnowmanGame());
var Guesses= new ConcurrentDictionary<int,int>();
Guesses.TryAdd(1,0);
Guesses.TryAdd(2,0);


app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/game/{gameid}", (int gameid) => {

    if(Games.ContainsKey(gameid)){

        var wordToGuess= Games.GetValueOrDefault(gameid);
        var guessesToWord= Guesses.GetValueOrDefault(gameid);

         return Results.Ok( new GetGame(wordToGuess!.Word,guessesToWord));

    }

    return Results.BadRequest();
    

})
.Produces<GetGame>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("GetGame")
.WithTags("Game")
.WithOpenApi(o=>{
    o.Description="returns the current word to guess and the number of already gueses";
    o.Summary="returns the status of the current game";
    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description="game data";
    o.Responses[((int)StatusCodes.Status400BadRequest).ToString()].Description="Gameid not found";
     return o;

});;

app.MapPost("/game",()=>{
    var newId= Games.Count+1;
    if(Games.TryAdd(newId,new MeltingSnowmanGame())){
        return Results.Ok(newId);
    }
    return Results.BadRequest();
}).Produces<GetGame>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("AddGame")
.WithTags("Game")
.WithOpenApi(o=>{
    o.Summary="creates a new game";
    o.Description="creates a new game and returnes the gameid";
    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description="new game id";
    o.Responses[((int)StatusCodes.Status400BadRequest).ToString()].Description="add new game failed";
     return o;

});


app.MapPost("/game/{gameid}",(int gameid,[FromBody]string letter)=>{

    if(Games.ContainsKey(gameid)){
        var wordToGuess = Games.GetValueOrDefault(gameid);
       Guesses[gameid]=Guesses.GetValueOrDefault(gameid)+1;
      var guessesToWord= Guesses.GetValueOrDefault(gameid);
    return Results.Ok( new PostGame(wordToGuess!.Guess(letter),wordToGuess!.Word,guessesToWord));

    }
    return Results.NotFound("Id nicht vorhanden");

    

}).Produces<GetGame>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PlayGame")
.WithTags("Game")
.WithOpenApi(o=>{
    o.Summary="try new letter for specific game";
    o.Description="try new letter for a specific game and returnes occurences of the letter,the word to guess and the guesses for the specific word ";
    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description="game data";
    o.Responses[((int)StatusCodes.Status400BadRequest).ToString()].Description="Gameid not found";
     return o;

});;


app.Run();
record GetGame(string WordToGuess,int NumberOfGuesses){
    public string WordToGuess{get;set;}=WordToGuess;
    public int NumberOfGuesses {get;set;}=NumberOfGuesses; 
}
record PostGame(int Occurences,string WordToGuess,int NumberOfGuesses){
    public int Occurences {get;set;}=Occurences;
    public string WordToGuess{get;set;}=WordToGuess;
    public int NumberOfGuesses {get;set;}=NumberOfGuesses; 

}




