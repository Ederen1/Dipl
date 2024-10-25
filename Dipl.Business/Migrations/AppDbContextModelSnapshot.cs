﻿// <auto-generated />
using System;
using Dipl.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Dipl.Business.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("Dipl.Business.Entities.Group", b =>
                {
                    b.Property<int>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("GroupId");

                    b.ToTable("Groups");

                    b.HasData(
                        new
                        {
                            GroupId = -1,
                            Description = "",
                            Name = "Guest"
                        });
                });

            modelBuilder.Entity("Dipl.Business.Entities.Link", b =>
                {
                    b.Property<Guid>("LinkId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Folder")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastAccessed")
                        .HasColumnType("TEXT");

                    b.Property<bool>("LinkClosed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LinkName")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int>("LinkType")
                        .HasColumnType("INT");

                    b.Property<string>("Message")
                        .HasMaxLength(10000)
                        .HasColumnType("TEXT");

                    b.Property<bool>("NotifyOnUpload")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PermissionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LinkId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("PermissionId");

                    b.ToTable("Links");
                });

            modelBuilder.Entity("Dipl.Business.Entities.Permission", b =>
                {
                    b.Property<int>("PermissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Read")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Write")
                        .HasColumnType("INTEGER");

                    b.HasKey("PermissionId");

                    b.HasIndex("GroupId")
                        .IsUnique();

                    b.ToTable("Permissions");

                    b.HasData(
                        new
                        {
                            PermissionId = -1,
                            GroupId = -1,
                            Read = true,
                            Write = false
                        });
                });

            modelBuilder.Entity("Dipl.Business.Entities.User", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserId = "f55aa676-775d-4312-b31c-e9d5848e06d7",
                            Email = "guest@example.com",
                            UserName = "Guest"
                        });
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.Property<int>("GroupsGroupId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UsersUserId")
                        .HasColumnType("TEXT");

                    b.HasKey("GroupsGroupId", "UsersUserId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("GroupUser");

                    b.HasData(
                        new
                        {
                            GroupsGroupId = -1,
                            UsersUserId = "f55aa676-775d-4312-b31c-e9d5848e06d7"
                        });
                });

            modelBuilder.Entity("PermissionUser", b =>
                {
                    b.Property<int>("PermissionsAssociatedWithThisUserPermissionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UsersUserId")
                        .HasColumnType("TEXT");

                    b.HasKey("PermissionsAssociatedWithThisUserPermissionId", "UsersUserId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("PermissionUser");
                });

            modelBuilder.Entity("Dipl.Business.Entities.Link", b =>
                {
                    b.HasOne("Dipl.Business.Entities.User", "CreatedBy")
                        .WithMany("Links")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dipl.Business.Entities.Permission", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("Permission");
                });

            modelBuilder.Entity("Dipl.Business.Entities.Permission", b =>
                {
                    b.HasOne("Dipl.Business.Entities.Group", "Group")
                        .WithOne("Permission")
                        .HasForeignKey("Dipl.Business.Entities.Permission", "GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.HasOne("Dipl.Business.Entities.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dipl.Business.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PermissionUser", b =>
                {
                    b.HasOne("Dipl.Business.Entities.Permission", null)
                        .WithMany()
                        .HasForeignKey("PermissionsAssociatedWithThisUserPermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dipl.Business.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Dipl.Business.Entities.Group", b =>
                {
                    b.Navigation("Permission")
                        .IsRequired();
                });

            modelBuilder.Entity("Dipl.Business.Entities.User", b =>
                {
                    b.Navigation("Links");
                });
#pragma warning restore 612, 618
        }
    }
}
