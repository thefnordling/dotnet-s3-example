# dotnet-s3-example
sample code for object CRUD in an S3 bucket (aws/minio/ceph/etc...)

just tinkering - a work in progress

# Instructions #

Clone this repository and start up a demo instance of minio

```
git clone https://github.com/thefnordling/dotnet-s3-example.git
cd ./dotnet-s3-example/docker
docker compose up -d
```

*This instance of minio is just for local development and testing, it is not secured or clustered - it should not be run in production or used to store anything sensitive or important.*

* Browse to [http://localhost:9001](http://localhost:9001)
* Log in with the username `minio` and the password `snickerdoodle`
* Under `Object Browser` - click `Create a Bucket` and call it `pail` and hit the `Create Bucket` button
* Under access keys click `Create access key +` and then `Create`.  As prompted - make note of the access key and secret key as they will not be viewable from the console again.
* Set two local environmental variables `AppSettings__AccessKey` and `AppSettings__SecretKey` and populate them with the respective values you copied from the previous step.  *You may need to restart your development tools for the new environmental variables to load*


