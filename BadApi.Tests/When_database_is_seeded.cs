using BadApi.Data;
using BadApi.OverPosting;
using Shouldly;

namespace BadApi.Tests;

public class When_database_is_seeded
{
    private readonly Database _database;
    
    public When_database_is_seeded()
    {
        _database = new Database();
        _database.Seed();
    }
    
    [Fact]
    public void It_should_seed_new_users()
    {
        // Act
        var result = _database.Users.List();

        // Assert
        result.ShouldNotBeEmpty();
        
        result[0].Id.ShouldBe(1);
        result[0].Name.ShouldBe("alice");
        result[0].Roles.ShouldBe(["developer"]);
    }
    
    [Fact]
    public async Task It_should_find_user_by_id()
    {
        // Act
        var result = await _database.Users.FindById(1);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Name.ShouldBe("alice");
        result.Roles.ShouldBe(["developer"]);
    }
    
    [Fact]
    public async Task It_should_insert_new_user_when_id_is_0()
    {
        // Act
        var result = _database.Users.Upsert(
            new UserEntity{ 
                Id = 0, 
                Name = "dave", 
                HashedPassword = "invalid", 
                Roles = ["developer"]
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(3);
        result.Name.ShouldBe("dave");
        result.Roles.ShouldBe(["developer"]);
    }
    
    [Fact]
    public async Task It_should_update_existing_user()
    {
        // Act
        var result = _database.Users.Upsert(
            new UserEntity{ 
                Id = 1, 
                Name = "Alicia", 
                HashedPassword = "invalid", 
                Roles = ["hacker"]
            });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        
        var user = await _database.Users.FindById(1);
        user.ShouldNotBeNull();
        user.Name.ShouldBe("Alicia");
        user.Roles.ShouldBe(["hacker"]);
        user.HashedPassword.ShouldNotBe("invalid", "Password should not be updated");
        user.HashedPassword.ShouldNotBeNull();
    }

    [Fact]
    public async Task It_should_set_the_name_of_a_user()
    {
        // Act
        var result = _database.Users.SetUserName(2, "newname");

        // Assert
        result.ShouldBeTrue();

        var user = await _database.Users.FindById(2);
        user.ShouldNotBeNull();
        user.Name.ShouldBe("newname");
    }
}