// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_person_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreatePersonModel _$CreatePersonModelFromJson(Map<String, dynamic> json) =>
    CreatePersonModel(
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      remarks: json['remarks'] as String?,
      contactInfo: json['contactInfo'] as String?,
      externalId: json['externalId'] as String?,
    );

Map<String, dynamic> _$CreatePersonModelToJson(CreatePersonModel instance) =>
    <String, dynamic>{
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'remarks': instance.remarks,
      'contactInfo': instance.contactInfo,
      'externalId': instance.externalId,
    };
