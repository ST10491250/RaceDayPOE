using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDayMVC.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
class Env : IWebHostEnvironment {
 public string ApplicationName {get;set;}="ImageChecks"; public string EnvironmentName{get;set;}="Development";
 public string ContentRootPath{get;set;}=""; public string WebRootPath{get;set;}="";
 public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider(); public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();
}
class Program {
 static void Check(bool condition,string message) {if(!condition)throw new Exception(message);}
 static IFormFile FileOf(byte[] bytes,string name) => new FormFile(new MemoryStream(bytes),0,bytes.Length,"image",name);
 static async Task Main() {
 var root=Path.Combine(Path.GetTempPath(),"raceday-image-tests-"+Guid.NewGuid().ToString("N"));
 var env=new Env{ContentRootPath=root,WebRootPath=Path.Combine(root,"wwwroot")};
 var store=new LocalImageStore(env,NullLogger<LocalImageStore>.Instance);
 try {
 var png=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a3ioAAAAASUVORK5CYII=");
 var image=await store.ValidateAsync(FileOf(png,"../../test.PNG"));Check(image!=null,"valid PNG rejected");
 await store.SaveAsync(ImageKind.Banners,31,image!);var first=store.GetPath(ImageKind.Banners,31);Check(first!=null && !first.Contains(".."),"invalid saved path");
 var reloaded=new LocalImageStore(env,NullLogger<LocalImageStore>.Instance);Check(reloaded.GetPath(ImageKind.Banners,31)==first,"reference did not persist");
 await store.SaveAsync(ImageKind.Banners,31,image!);Check(store.GetPath(ImageKind.Banners,31)!=first,"replacement reused URL");Check(Directory.GetFiles(Path.Combine(env.WebRootPath,"uploads","banners")).Length==1,"old image not cleaned up");
 await store.SaveAsync(ImageKind.Profiles,7,image!);Check(store.GetPath(ImageKind.Profiles,7)!=null,"profile missing");Check(store.GetPath(ImageKind.Profiles,8)==null,"image leaked between user IDs");
 foreach(var file in new[]{FileOf(new byte[0],"empty.jpg"),FileOf(new byte[25],"fake.png"),FileOf(png,"bad.svg"),FileOf(new byte[LocalImageStore.MaxBytes+1],"large.jpg")}) {
 bool rejected=false;try{await store.ValidateAsync(file);}catch(ImageUploadException){rejected=true;}Check(rejected,"invalid upload accepted");
 }
 Check(await store.ValidateAsync(null)==null,"optional image rejected");
 Console.WriteLine("PASS: image validation, size limits, safe filenames, persistent loading, replacement cleanup, optional uploads and user isolation.");
 }finally{if(Directory.Exists(root))Directory.Delete(root,true);}
 }
}
